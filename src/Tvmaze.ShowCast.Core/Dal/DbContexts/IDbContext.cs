namespace Tvmaze.ShowCast.Core.Dal.DbContexts;

public interface IDbContext
{
    Task UpsertAsync<T>(T item, CancellationToken token);

    Task<IList<T>> QueryAsync<T>(string queryText, CancellationToken token);
}