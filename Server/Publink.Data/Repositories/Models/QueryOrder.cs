namespace Publink.Data.Repositories.Models;

public enum OrderDirection
{
    Asc,
    Desc
}

public record OrderByQuery<TEntity>(Func<TEntity, object> KeySelector, OrderDirection Direction) where TEntity : class;