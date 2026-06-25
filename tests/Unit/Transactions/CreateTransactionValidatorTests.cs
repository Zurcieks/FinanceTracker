

using Api.Domain;
using Api.Features.Transactions.Create;
using FluentValidation.TestHelper;

namespace Tests.Unit.Transactions;

public class CreateTransactionValidatorTests
{
    private readonly CreateTransactionValidator _validator = new();

    // Valid helper
    private static CreateTransactionRequest ValidRequest() => new(
        MerchantName: "Żabka",
        Description: null,
        Amount: 24.99m,
        Currency: TransactionCurrency.PLN,
        Type: TransactionType.Expense,
        ReceiptKey: null,
        Date: null,
        CategoryId: Guid.NewGuid());

    [Fact]
    public void Should_NotHaveError_When_RequestIsValid()
    {
        var result = _validator.TestValidate(ValidRequest());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Should_HaveError_When_AmountNotPositive(decimal amount)
    {
        var request = ValidRequest() with { Amount = amount };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Amount);
    }

    [Fact]
    public void Should_HaveError_When_MerchantNameEmpty()
    {
        var request = ValidRequest() with { MerchantName = "" };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.MerchantName);
    }

    [Fact]
    public void Should_HaveError_When_CategoryIdEmpty()
    {
        var request = ValidRequest() with { CategoryId = Guid.Empty };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.CategoryId);
    }
}
