using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Publink.Data.Context;
using Publink.Data.Entities;
using Publink.Data.Repositories.Models;

namespace Publink.Data.Repositories;

public interface IAuditLogRepository
{
    /// <summary>
    /// Returns a list of AuditLogs with pagination, order and filter.
    /// </summary>
    /// <param name="filter"></param>
    /// <param name="pagination"></param>
    /// <param name="order"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    IEnumerable<AuditLog> GetList(Func<AuditLog, bool>? filter, QueryPagination? pagination,
        OrderByQuery<AuditLog>? order, CancellationToken cancellationToken = default);

    /// <summary>
    /// Count audit logs with optional filter.
    /// </summary>
    /// <param name="filter"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<int> CountAsync(Func<AuditLog, bool>? filter, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns aggregates (affected rows count and time span) grouped by CorrelationId for the given set.
    /// </summary>
    /// <param name="correlationIds">Set of correlation IDs to aggregate.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IReadOnlyDictionary<Guid, ChangeAggregate>> GetAggregatesByCorrelationIdsAsync(IEnumerable<Guid> correlationIds, CancellationToken cancellationToken = default);
}

public class AuditLogRepository : IAuditLogRepository
{
    private readonly PublinkDbContext _db;
    private readonly ILogger<AuditLogRepository> _logger;

    public AuditLogRepository(PublinkDbContext db, ILogger<AuditLogRepository> logger)
    {
        _db = db;
        _logger = logger;
    }

    // <inheritdoc />
    public IEnumerable<AuditLog> GetList(Func<AuditLog, bool>? filter, QueryPagination? pagination, OrderByQuery<AuditLog>? order, CancellationToken cancellationToken = default)
    {
        try
        {
            pagination ??= new QueryPagination()
            {
                Skip = 0,
                Take = 100
            };
            order ??= new OrderByQuery<AuditLog>(x => x.CreatedDate, OrderDirection.Desc);

            var query = filter is null
                ? _db.AuditLogs.AsQueryable()
                : _db.AuditLogs.Where(filter).AsQueryable();

            query = order.Direction is OrderDirection.Asc
                ? query.AsEnumerable().OrderBy(order.KeySelector).AsQueryable()
                : query.AsEnumerable().OrderByDescending(order.KeySelector).AsQueryable();

            return query.Skip(pagination.Skip)
                .Take(pagination.Take)
                .Include(log => log.DocumentHeader)
                .AsEnumerable();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error getting list of audit logs");
            throw;
        }
    }

    // <inheritdoc />
    public async Task<int> CountAsync(Func<AuditLog, bool>? filter, CancellationToken cancellationToken = default)
    {
        if(filter is null)
            return await _db.AuditLogs.CountAsync(cancellationToken);

        return _db.AuditLogs.Where(filter).Count();
    }

    public async Task<IReadOnlyDictionary<Guid, ChangeAggregate>> GetAggregatesByCorrelationIdsAsync(IEnumerable<Guid> correlationIds, CancellationToken cancellationToken = default)
    {
        try
        {
            var ids = correlationIds.Distinct().ToList();
            if (ids.Count == 0)
                return new Dictionary<Guid, ChangeAggregate>();

            var aggregates = await _db.AuditLogs
                .AsNoTracking()
                .Where(a => a.CorrelationId.HasValue && ids.Contains(a.CorrelationId.Value))
                .GroupBy(a => a.CorrelationId!.Value)
                .Select(g => new ChangeAggregate
                {
                    CorrelationId = g.Key,
                    Count = g.Count(),
                    Start = g.Min(x => x.CreatedDate),
                    End = g.Max(x => x.CreatedDate)
                })
                .ToDictionaryAsync(a => a.CorrelationId, a => a, cancellationToken);

            return aggregates;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error getting aggregates for audit logs by correlation IDs");
            throw;
        }
    }
}