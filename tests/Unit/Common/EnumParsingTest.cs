using Api.Common.Extensions;
using Api.Domain;

namespace Tests.Unit.Common;


public class EnumParsingTests
{
    [Theory]
    [InlineData("PLN", TransactionCurrency.PLN)]
    [InlineData("EUR", TransactionCurrency.EUR)]
    [InlineData("pln", TransactionCurrency.PLN)]
    public void ParseOrNull_ReturnsEnum_When_ValueIsValid(string input, TransactionCurrency expected)
    {
        var result = EnumParsing.ParseOrNull<TransactionCurrency>(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("USD")]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("xyz")]
    public void ParseOrNull_ReturnsNull_When_ValueIsInvalid(string? input)
    {
        var result = EnumParsing.ParseOrNull<TransactionCurrency>(input);
        Assert.Null(result);
    }


}
