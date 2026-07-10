using System.Globalization;

namespace Api.Common.Extensions;

public static class DateParsing
{
    public static bool TryParseOptional(string? input, out DateOnly? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return true;
        if (!DateOnly.TryParseExact(input, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
            return false; // bad format
        result = parsed;
        return true;
    }
}
