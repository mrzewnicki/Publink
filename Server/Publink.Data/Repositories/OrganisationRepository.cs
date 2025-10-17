using Microsoft.EntityFrameworkCore;
using Publink.Data.Context;
using Publink.Data.Repositories.Models;

namespace Publink.Data.Repositories;

public interface IOrganisationRepository
{
    /// <summary>
    /// Query for all available Organisations names
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IEnumerable<OrganisationItem>> GetNamesAllAsync(CancellationToken cancellationToken = default);
}

public class OrganisationRepository : IOrganisationRepository
{
    private readonly PublinkDbContext _db;

    public OrganisationRepository(PublinkDbContext db)
    {
        _db = db;
    }

    // <inheritdoc />
    public async Task<IEnumerable<OrganisationItem>> GetNamesAllAsync(CancellationToken cancellationToken = default)
    {
        // I use ContractorName as a best-effort label cuz there is no organisation table with data i could join
        return await _db.DocumentHeaders
            .AsNoTracking()
            .Where(d => d.OrganizationId != Guid.Empty)
            .GroupBy(d => d.OrganizationId)
            .Select(g => new OrganisationItem
            {
                Id = g.Key,
                Name = g.Select(x => x.ContractorName)
                        .FirstOrDefault(n => n != null && n.Trim() != string.Empty) ?? string.Empty
            })
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }
}
