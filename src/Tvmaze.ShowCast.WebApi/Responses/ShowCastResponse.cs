using Tvmaze.ShowCast.WebApi.Models;

namespace Tvmaze.ShowCast.WebApi.Responses;

public record ShowCastResponse(int Id, string Name, IEnumerable<ShowCastCastModel>? Cast);