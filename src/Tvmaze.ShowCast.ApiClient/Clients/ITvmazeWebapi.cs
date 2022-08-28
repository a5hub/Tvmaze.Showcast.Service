using Refit;
using Tvmaze.ShowCast.ApiClient.Responses;

namespace Tvmaze.ShowCast.ApiClient.Clients;

/// <summary> Access to TvMaze Web API </summary>
/// <remarks> API calls are rate limited to allow at least 20 calls every 10 seconds per IP address </remarks>
public interface ITvmazeWebapi
{
    /// <summary> A paginated list of all existing shows </summary>
    /// <remarks> This endpoint is paginated, with a maximum of 250 results per page </remarks>
    /// <remarks> During iteration HTTP 404 response code indicates that the end of the list reached </remarks>
    /// <param name="pageNum"> Page number, starts from 0 </param>
    /// <param name="token"> Cancellation token </param>
    /// <returns> Collection of 250 shows per page </returns>
    [Get("/shows?page={pageNum}")]
    Task<IEnumerable<GetShowsResponse>> GetShowsAsync(int pageNum, CancellationToken token);

    /// <summary>
    /// A list of main cast for a show. Each cast item is a combination of a person and a character.
    /// Items are ordered by importance, which is determined by the total number of appearances of the given character in this show
    /// </summary>
    /// <param name="showId"> Show id</param>
    /// <param name="token"> Cancellation token </param>
    /// <returns> Collection of show actors </returns>
    [Get("/shows/{showId}/cast")]
    Task<IEnumerable<GetCastResponse>> GetCastAsync(int showId, CancellationToken token);
}