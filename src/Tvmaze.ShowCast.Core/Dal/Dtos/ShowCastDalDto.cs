namespace Tvmaze.ShowCast.Core.Dal.Dtos;

public record ShowCastDalDto(int Id, string Name, IEnumerable<CastDalDto>? Cast);