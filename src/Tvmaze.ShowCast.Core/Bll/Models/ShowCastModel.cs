namespace Tvmaze.ShowCast.Core.Bll.Models;

public record ShowCastModel (int Id, string Name, IEnumerable<CastModel>? Cast);