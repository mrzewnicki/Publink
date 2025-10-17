using System.Net;
using MediatR;
using Publink.Data.Entities;
using Publink.Data.Repositories;
using Publink.Data.Repositories.Models;
using Publink.Shared.Dtos;

namespace Publink.Core.CQRS.Queries;

public static class GetLogsForOrganisationQuery
{
    public sealed record Request(QueryDto Query, Guid OrganizationId) : IRequest<Response>;

    public sealed record Response(IEnumerable<AuditLogDto> Items, int TotalCount, HttpStatusCode StatusCode = HttpStatusCode.OK);

    public sealed class Handler(IAuditLogRepository repository) : IRequestHandler<Request, Response>
    {
        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            if(request.OrganizationId == Guid.Empty)
                return new Response([], 0, HttpStatusCode.BadRequest);

            var pageSize = request.Query.PageSize <= 0 ? 10 : request.Query.PageSize;
            var pageNumber = request.Query.PageNumber <= 0 ? 1 : request.Query.PageNumber;
            var skip = (pageNumber - 1) * pageSize;

            var order = ParseSort(request.Query.Sort);
            var pagination = new QueryPagination
            {
                Skip = skip,
                Take = pageSize
            };

            Func<AuditLog, bool> predicate = log => log.OrganizationId == request.OrganizationId;

            var entities = repository.GetList(predicate, pagination, order).ToList();

            // Aggregate by correlation IDs to compute affected rows and time span
            var correlationIds = entities
                .Where(e => e.CorrelationId.HasValue)
                .Select(e => e.CorrelationId!.Value)
                .Distinct()
                .ToList();

            var aggregates = await repository.GetAggregatesByCorrelationIdsAsync(correlationIds, cancellationToken);

            var items = entities.Select(e => MapToDto(e, aggregates)).AsEnumerable();
            var totalCount = await repository.CountAsync(predicate, cancellationToken);

            return new Response(items, totalCount);
        }

        private static AuditLogDto MapToDto(AuditLog e, IReadOnlyDictionary<Guid, ChangeAggregate> aggregates)
        {
            var count = 1;
            var duration = TimeSpan.Zero;

            if (e.CorrelationId.HasValue && aggregates.TryGetValue(e.CorrelationId.Value, out var agg))
            {
                count = agg.Count;
                duration = agg.Duration;
            }

            return new AuditLogDto
            {
                Id = e.Id,
                ChangedBy = e.UserEmail,
                ContractNumber = e.DocumentHeader != null && e.DocumentHeader.Number != null ? e.DocumentHeader.Number : string.Empty,
                Type = (int)e.Type,
                EntityType = (int)e.EntityType,
                CreatedDate = e.CreatedDate,
                // Encode duration as a DateTime based on ticks (DTO uses DateTime; keeping API stable)
                ProcessTookTime = new DateTime(duration.Ticks, DateTimeKind.Utc),
                EntitiesAffectCount = count
            };
        }

        private static OrderByQuery<AuditLog> ParseSort(string? sort)
        {
            if (string.IsNullOrWhiteSpace(sort))
                return new OrderByQuery<AuditLog>(log => log.CreatedDate, OrderDirection.Desc);

            var parts = sort.Split(':', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var prop = parts.Length > 0 ? parts[0].ToLowerInvariant() : string.Empty;
            var dir = parts.Length > 1 ? parts[1].ToLowerInvariant() : "desc";

            var direction = dir == "asc" ? OrderDirection.Asc : OrderDirection.Desc;

            return CreateOrderByQuery(prop, direction);
        }

        private static OrderByQuery<AuditLog> CreateOrderByQuery(string propertyName, OrderDirection direction)
        {
            // Add more fields to let order by'em
            Func<AuditLog, object> orderQuery = propertyName switch
            {
                nameof(AuditLog.CreatedDate) => log => log.CreatedDate,
                _ => log => log.CreatedDate,
            };

            return new OrderByQuery<AuditLog>(orderQuery, direction);
        }
    }
}