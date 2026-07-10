using Api.Common.Extensions;

namespace Tests.Unit.Common;

public class DateParsingTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void TryParseOptional_ReturnsTrueWithNull_WhenValueIsMissing(string? value)
    {
        var success = DateParsing.TryParseOptional(value, out var result);

        Assert.True(success);
        Assert.Null(result);

    }

    [Fact]
    public void TryParseOptional_ParsesIsoDate()
    {
        var success = DateParsing.TryParseOptional("2026-07-10", out var result);

        Assert.True(success);
        Assert.Equal(new DateOnly(2026, 7, 10), result);
    }

    [Theory]
    [InlineData("07/10/2026")] // format zależny od kultury (US: 10 lipca, EU: 7 stycznia) - musi zostać odrzucony
    [InlineData("10.07.2026")]
    [InlineData("2026/07/10")]
    [InlineData("nie-data")]
    [InlineData("2026-13-45")]
    public void TryParseOptional_RejectsNonIsoFormat_RegardlessOfHostCulture(string value)
    {
        var success = DateParsing.TryParseOptional(value, out var result);

        Assert.False(success);
        Assert.Null(result);
    }
}
