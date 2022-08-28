using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Tvmaze.ShowCast.ApiClient.Clients;
using Tvmaze.ShowCast.ApiClient.Responses;
using Tvmaze.ShowCast.Core.Bll.Services;
using Tvmaze.ShowCast.Core.Dal.Dtos;
using Tvmaze.ShowCast.Core.Dal.Repository;
using Tvmaze.ShowCast.Core.Extensions;
using Tvmaze.ShowCast.Core.Options;

namespace Tvmaze.ShowCast.Core.Tests.Bll.Services;

public class ShowCastServiceTests
{
    private readonly Mock<IShowCastRepository> _showCastRepository;

    private readonly Mock<ITvmazeWebapi> _tvmazeWebapi;

    private readonly IShowCastService _sut;
    
    public ShowCastServiceTests()
    {
        _showCastRepository = new Mock<IShowCastRepository>();
        _tvmazeWebapi = new Mock<ITvmazeWebapi>();
        var logger = new Mock<ILogger<ShowCastService>>();
        var parallelismOptions = Microsoft.Extensions.Options.Options.Create(new ParallelismOptions
        {
            TvmazeApiMaxDegreeOfParallelism = 1
        });
        _sut = new ShowCastService(_showCastRepository.Object, _tvmazeWebapi.Object,
            logger.Object, parallelismOptions);
    }
    
    [Fact]
    public async Task CollectDataSequentially_NoShows()
    {
        // Arrange
        var token = CancellationToken.None;
        var lastRecordId = 255;
        var lastSyncedPage = 1;

        var getShowsResponse = new List<GetShowsResponse>();
        
        // Expectations
        _showCastRepository.Setup(x => x.GetLastRecordIdAsync(token)).ReturnsAsync(lastRecordId);
        _tvmazeWebapi.Setup(x => x.GetShowsAsync(lastSyncedPage, token)).ReturnsAsync(getShowsResponse);
        
        // Act
        await _sut.CollectDataSequentially(token);

        // Assert
        _tvmazeWebapi.Verify(mock => mock.GetCastAsync(It.IsAny<int>(), token), Times.Never);
        _showCastRepository.Verify(mock => 
            mock.UpsertItemAsync(It.IsAny<ShowCastDalDto>(), It.IsAny<int>(), token), Times.Never);
    }
    
    [Fact]
    public async Task CollectDataSequentially_NoCast()
    {
        // Arrange
        var token = CancellationToken.None;
        var lastRecordId = 255;
        var lastSyncedPage = 1;

        var getShowsResponse = new List<GetShowsResponse>
        {
            new() {Id = 1, Name = "One" },
            new() {Id = 2, Name = "Two" }
        };

        var cast = new List<GetCastResponse>();
        
        // Expectations
        _showCastRepository.Setup(x => x.GetLastRecordIdAsync(token)).ReturnsAsync(lastRecordId);
        _tvmazeWebapi.Setup(x => x.GetShowsAsync(lastSyncedPage, token)).ReturnsAsync(getShowsResponse);
        _tvmazeWebapi.Setup(x => x.GetCastAsync(It.IsAny<int>(), token)).ReturnsAsync(cast);
        _showCastRepository.Setup(x => x.UpsertItemAsync(It.IsAny<ShowCastDalDto>(), lastSyncedPage, token))
            .Callback((ShowCastDalDto showCast, int pageNumber, CancellationToken token) =>
            {
                Assert.Equal(showCast.Cast.Count(), 0);
            });
        
        // Act
        await _sut.CollectDataSequentially(token);
        
        // Assert
        _showCastRepository.VerifyAll();
        _tvmazeWebapi.VerifyAll();
        _showCastRepository.VerifyAll();
    }
    
    [Fact]
    public async Task CollectDataSequentially_WorksFine()
    {
        // Arrange
        var token = CancellationToken.None;
        var lastRecordId = 255;
        var lastSyncedPage = 1;

        var getShowsResponse = new List<GetShowsResponse>
        {
            new() {Id = 1, Name = "One" },
            new() {Id = 2, Name = "Two" }
        };

        var castResponse1 = new List<GetCastResponse>
        {
            new()
            {
                Persons = new GetCastResponse.Person
                {
                    Id = 1, Name = "person 1", Birthday = "1990-01-01"
                }
            },
            new()
            {
                Persons = new GetCastResponse.Person
                {
                    Id = 2, Name = "person 2", Birthday = "1970-01-01"
                }
            }
        };
        var castResponse2 = new List<GetCastResponse>
        {
            new()
            {
                Persons = new GetCastResponse.Person
                {
                    Id = 3, Name = "person 3", Birthday = "1977-01-01"
                }
            },
            new()
            {
                Persons = new GetCastResponse.Person
                {
                    Id = 4, Name = "person 4", Birthday = "1987-01-01"
                }
            }
        };
        
        var showCast1 = new ShowCastDalDto(getShowsResponse[0].Id, getShowsResponse[0].Name, 
            new List<CastDalDto>
            {
                new (castResponse1[0].Persons.Id, castResponse1[0].Persons.Name, castResponse1[0].Persons.Birthday.ToDateOnly()),
                new (castResponse1[1].Persons.Id, castResponse1[1].Persons.Name, castResponse1[1].Persons.Birthday.ToDateOnly())
            });
        var showCast2 = new ShowCastDalDto(getShowsResponse[1].Id, getShowsResponse[1].Name, new List<CastDalDto>
        {
            new (castResponse2[0].Persons.Id, castResponse2[0].Persons.Name, castResponse2[0].Persons.Birthday.ToDateOnly()),
            new (castResponse2[1].Persons.Id, castResponse2[1].Persons.Name, castResponse2[1].Persons.Birthday.ToDateOnly())
        });
        
        // Expectations
        _showCastRepository.Setup(x => x.GetLastRecordIdAsync(token)).ReturnsAsync(lastRecordId);
        _tvmazeWebapi.Setup(x => x.GetShowsAsync(lastSyncedPage, token)).ReturnsAsync(getShowsResponse);
        _tvmazeWebapi.SetupSequence(x => x.GetCastAsync(It.IsAny<int>(), token))
            .ReturnsAsync(castResponse1)
            .ReturnsAsync(castResponse2);

        var sec = 0;
        _showCastRepository.Setup(x => x.UpsertItemAsync(It.IsAny<ShowCastDalDto>(), lastSyncedPage, token))
            .Callback((ShowCastDalDto showCast, int pageNumber, CancellationToken token) =>
            {
                if(sec == 0) showCast.Should().BeEquivalentTo(showCast1);
                if(sec == 1) showCast.Should().BeEquivalentTo(showCast2);
                pageNumber.Should().Be(lastSyncedPage);
                sec++;
            });
        
        // Act
        await _sut.CollectDataSequentially(token);
        
        // Assert
        _showCastRepository.VerifyAll();
        _tvmazeWebapi.VerifyAll();
        _showCastRepository.VerifyAll();
    }
}