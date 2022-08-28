using Tvmaze.ShowCast.Core.Dal.DbContexts;
using Tvmaze.ShowCast.Core.Dal.Dtos;
using Tvmaze.ShowCast.Core.Dal.Entities;
using Tvmaze.ShowCast.Core.Extensions;

namespace Tvmaze.ShowCast.Core.Dal.Repository;

public class ShowCastRepository : IShowCastRepository
{
    private readonly IDbContext _dbContext;

    public ShowCastRepository(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task UpsertItemAsync(ShowCastDalDto dalDto, int pageNumber, CancellationToken token)
    {
        var item = new ShowCastEntity
        {
            Id = dalDto.Id.ToString(),
            PartitionKey = pageNumber.ToString(),
            Name = dalDto.Name,
            Cast = dalDto.Cast?.Select(x => new Cast
            {
                Id = x.Id,
                Name = x.Name,
                Birthday = x.BirthDay.ToString()
            })
        };
        
        await _dbContext.UpsertAsync(item, token);
    }

    public async Task<IEnumerable<ShowCastDalDto>> GetItemsPageAsync(int pageNumber, CancellationToken token)
    {
        var queryText = $"SELECT * FROM c WHERE c.partitionKey = '{pageNumber}'";
        var data = await _dbContext.QueryAsync<ShowCastEntity>(queryText, token);

        return data.Select(x => new ShowCastDalDto(Int32.Parse(x.Id), x.Name, 
             x.Cast?.Select(y => new CastDalDto(y.Id, y.Name, y.Birthday?.ToDateOnly())))
        );
    }

    public async Task<int> GetLastRecordIdAsync(CancellationToken token)
    {
        var queryText = "SELECT MAX(c.Id) Id FROM c";
        var maxId = (await _dbContext.QueryAsync<MaxId>(queryText, token)).SingleOrDefault();

        return maxId.Id;
    }
}