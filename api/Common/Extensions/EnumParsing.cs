namespace Api.Common.Extensions;

public static class EnumParsing
{
    public static TEnum? ParseOrNull<TEnum>(string? value) where TEnum : struct, Enum =>
        Enum.TryParse<TEnum>(value, ignoreCase: true, out var result) ? result : null;
}
