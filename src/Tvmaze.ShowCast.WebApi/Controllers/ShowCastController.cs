using Microsoft.AspNetCore.Mvc;
using Tvmaze.ShowCast.WebApi.ApiServices;
using Tvmaze.ShowCast.WebApi.Responses;

namespace Tvmaze.ShowCast.WebApi.Controllers;

[ApiController]
[Route("api/showcast")]
public class ShowCastController : ControllerBase
{
    private readonly IShowCastApiService _showCastApiService;

    public ShowCastController(IShowCastApiService showCastApiService)
    {
        _showCastApiService = showCastApiService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ShowCastResponse>>> Get(int page, CancellationToken token)
    {
        var result = await _showCastApiService.GetSortedDescPage(page, token);
        return Ok(result);
    }
}