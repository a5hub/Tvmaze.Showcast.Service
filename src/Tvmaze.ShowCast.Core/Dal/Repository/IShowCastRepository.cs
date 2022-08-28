using Tvmaze.ShowCast.Core.Dal.Dtos;

namespace Tvmaze.ShowCast.Core.Dal.Repository;

public interface IShowCastRepository
{
    Task UpsertItemAsync(ShowCastDalDto dto, int pageNumber, CancellationToken token);

    Task<IEnumerable<ShowCastDalDto>> GetItemsPageAsync(int pageNumber, CancellationToken token);

    Task<int> GetLastRecordIdAsync(CancellationToken token);
}