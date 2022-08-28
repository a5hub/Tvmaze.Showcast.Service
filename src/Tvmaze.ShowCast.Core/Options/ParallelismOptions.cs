namespace Tvmaze.ShowCast.Core.Options;

public class ParallelismOptions
{
    public const string Key = "Parallelism";
    
    public int TvmazeApiMaxDegreeOfParallelism { get; set; }
}