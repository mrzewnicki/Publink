using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Publink.Data.Context;
using Publink.Data.Entities;
using Publink.Data.Repositories.Models;

namespace Publink.Data.Repositories;

public interface IAuditLogRepository
{
    IEnumerable<AuditLog> GetList(QueryPagination? pagination, OrderByQuery<AuditLog>? order,
        CancellationToken cancellationToken = default);
    Task<int> CountAsync(CancellationToken cancellationToken = default);
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

    public IEnumerable<AuditLog> GetList(QueryPagination? pagination, OrderByQuery<AuditLog>? order, CancellationToken cancellationToken = default)
    {
        try
        {
            pagination ??= new QueryPagination()
            {
                Skip = 0,
                Take = 100
            };
            order ??= new OrderByQuery<AuditLog>(x => x.CreatedDate, OrderDirection.Desc);

            var query = order.Direction is OrderDirection.Asc
                ? _db.AuditLogs.OrderBy(order.KeySelector)
                : _db.AuditLogs.OrderByDescending(order.KeySelector);

            return query.Skip(pagination.Skip)
                .Take(pagination.Take)
                .AsEnumerable();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error getting list of audit logs");
            throw;
        }
    }

    public Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return _db.AuditLogs.CountAsync(cancellationToken);
    }
}