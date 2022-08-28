using FluentAssertions;
using Moq;
using Tvmaze.ShowCast.Core.Bll.Models;
using Tvmaze.ShowCast.Core.Bll.Services;
using Tvmaze.ShowCast.WebApi.ApiServices;
using Tvmaze.ShowCast.WebApi.Cache;
using Tvmaze.ShowCast.WebApi.Responses;

namespace Tvmaze.ShowCast.WebApi.Tests.ApiServices;

public class ShowCastApiServiceTests
{
    private readonly Mock<IShowCastService> _showCastService;
    private readonly Mock<ICache> _cache;
    private readonly ShowCastApiService _sut;

    public ShowCastApiServiceTests()
    {
        _showCastService = new Mock<IShowCastService>();
        _cache = new Mock<ICache>();
        _sut = new ShowCastApiService(_showCastService.Object, _cache.Object);
    }
    
    [Fact]
    private async Task GetSortedDescPage_SortingWorksFine()
    {
        // Arrange
        var page = 1;

        var castOld = new CastModel(1, "cast 1", new DateOnly(1950, 1, 1));
        var castYounger = new CastModel(2, "cast 2", new DateOnly(1959, 1, 1));
        var castYoungest = new CastModel(3, "cast 3", new DateOnly(1977, 1, 1));

        var showCast = new List<ShowCastModel> 
        {         
            new (1, "show 1", new List<CastModel> { castOld, castYounger, castYoungest }),
            new (2, "show 2", new List<CastModel> { castYoungest, castYounger, castOld }),
            new (3, "show 3", new List<CastModel> { castOld, castYoungest, castYounger })
        };

        IEnumerable<ShowCastResponse>? cacheResponse = null;
        // Expectations
        _cache.Setup(x => x.GetAsync<IEnumerable<ShowCastResponse>>(page.ToString(), CancellationToken.None))
            .ReturnsAsync(cacheResponse);
        _showCastService.Setup(x => x.GetPageAsync(page, CancellationToken.None)).ReturnsAsync(showCast);
        
        // Act
        var actual = await _sut.GetSortedDescPage(page, CancellationToken.None);
        
        // Assert
        actual.Select(x => x.Cast.Should().BeInDescendingOrder(x => x.BirthDay));
        _showCastService.Verify(mock => mock.GetPageAsync(page, CancellationToken.None), Times.Once());
    }
}