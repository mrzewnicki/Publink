using MediatR;
using Publink.Data.Entities;
using Publink.Data.Repositories;
using Publink.Data.Repositories.Models;
using Publink.Shared.Dtos;

namespace Publink.Core.CQRS.Queries;

public static class GetLogsQuery
{
    public sealed record Request(QueryDto Query) : IRequest<Response>;

    public sealed record Response(IEnumerable<AuditLogDto> Items, int TotalCount);

    public sealed class Handler(IAuditLogRepository repository) : IRequestHandler<Request, Response>
    {
        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            var pageSize = request.Query.PageSize <= 0 ? 10 : request.Query.PageSize;
            var pageNumber = request.Query.PageNumber <= 0 ? 1 : request.Query.PageNumber;
            var skip = (pageNumber - 1) * pageSize;

            var order = ParseSort(request.Query.Sort);
            var pagination = new QueryPagination
            {
                Skip = skip,
                Take = pageSize
            };

            var entities = repository.GetList(pagination, order);
            var items = entities.Select(MapToDto).AsEnumerable();
            var totalCount = await repository.CountAsync(cancellationToken);

            return new Response(items, totalCount);
        }

        private static AuditLogDto MapToDto(AuditLog e) => new AuditLogDto
        {
            Id = e.Id,
            OrganizationId = e.OrganizationId,
            UserId = e.UserId,
            UserEmail = e.UserEmail,
            Type = (int)e.Type,
            EntityType = (int)e.EntityType,
            CreatedDate = e.CreatedDate,
            OldValues = e.OldValues,
            NewValues = e.NewValues,
            AffectedColumns = e.AffectedColumns,
            PrimaryKey = e.PrimaryKey,
            EntityId = e.EntityId,
            ParentId = e.ParentId,
            CorrelationId = e.CorrelationId,
            SubUnitId = e.SubUnitId
        };

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