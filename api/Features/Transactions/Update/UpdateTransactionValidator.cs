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
            .GreaterThan(0).WithMessage("Amount must be greater than 0");   // bez NotEmpty

        RuleFor(x => x.Currency).IsInEnum().WithMessage("Invalid currency"); // bez NotEmpty
        RuleFor(x => x.Type).IsInEnum().WithMessage("Invalid transaction type");

        RuleFor(x => x.CategoryId).NotEmpty().WithMessage("Category is required");
    }
}
