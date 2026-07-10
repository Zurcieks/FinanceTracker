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
}
