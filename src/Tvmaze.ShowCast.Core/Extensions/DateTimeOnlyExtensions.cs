namespace Tvmaze.ShowCast.Core.Extensions;

public static class DateTimeOnlyExtensions
{
    public static DateOnly? ToDateOnly(this string? str)
    {
        DateOnly.TryParse(str, out var date);
        return date;
    }
}