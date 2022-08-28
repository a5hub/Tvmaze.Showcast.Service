using Tvmaze.ShowCast.Core.Bll.Models;

namespace Tvmaze.ShowCast.Core.Bll.Services;

public interface IShowCastService
{
    /// <summary> Collect all information about showcast </summary>
    /// <remarks> Starts from last record page </remarks>
    /// <param name="token"> Cancellation token </param>
    Task CollectDataSequentially(CancellationToken token);

    /// <summary> Get data by page number </summary>
    /// <param name="pageNumber"> Page number 0 ... </param>
    /// <param name="token"> Cancellation token </param>
    /// <returns> Collection of show cast records </returns>
    /// <remarks> Page size is 250 </remarks>
    Task<IEnumerable<ShowCastModel>> GetPageAsync(int pageNumber, CancellationToken token);
}