namespace Api.Common.Extensions;

public static class DateParsing
{
    public static bool TryParseOptional(string? input, out DateOnly? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return true;
        if (!DateOnly.TryParse(input, out var parsed)) return false; // bad format
        result = parsed;
        return true;
    }
}
