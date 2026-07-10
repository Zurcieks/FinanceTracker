using FluentValidation;
namespace Api.Features.Transactions.Update;

public class UpdateTransactionValidator : AbstractValidator<UpdateTransactionRequest>
{
    public UpdateTransactionValidator()
    {
        RuleFor(x => x.MerchantName)
            .NotEmpty().WithMessage("Merchant name is required")
            .MaximumLength(100).WithMessage("Merchant name cannot be longer than 100 characters");
        RuleFor(x => x.Description)
            .MaximumLength(200).WithMessage("Description cannot be longer than 200 characters");
        RuleFor(x => x.Amount)
            .NotEmpty().WithMessage("Amount is required")
            .GreaterThan(0).WithMessage("Amount must be greater than 0")
            .Must(amount => amount == decimal.Round(amount, 2)).WithMessage("Amount can have at most 2 decimal places");
        RuleFor(x => x.Currency).IsInEnum().WithMessage("Invalid currency");
        RuleFor(x => x.Type).IsInEnum().WithMessage("Invalid transaction type");
        RuleFor(x => x.CategoryId).NotEmpty().WithMessage("Category is required");
    }
}
