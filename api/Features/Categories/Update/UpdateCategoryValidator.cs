using FluentValidation;
namespace Api.Features.Categories.Update;

public class UpdateCategoryValidator : AbstractValidator<UpdateCategoryRequest>
{
    public UpdateCategoryValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name cannot be longer than 100 characters");

        RuleFor(x => x.HexColor)
            .NotEmpty().WithMessage("HexColor is required")
            .Matches(@"^#[0-9A-Fa-f]{6}$").WithMessage("HexColor must be a valid hex color, e.g. #FF5733");

        RuleFor(x => x.Icon)
            .NotEmpty().WithMessage("Icon is required")
            .MaximumLength(50).WithMessage("Icon cannot be longer than 50 characters");
    }
}
