using FluentValidation;

namespace Api.Features.Categories.Create;

public class CreateCategoryValidator : AbstractValidator<CreateCategoryRequest>
{
    public CreateCategoryValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);


        RuleFor(x => x.HexColor)
            .NotEmpty()
            .Matches(@"^#[0-9A-Fa-f]{6}$")
            .WithMessage("HexColor must be a valid hex color, e.g. #FF5733.");
        RuleFor(x => x.Icon)
            .NotEmpty()
            .MaximumLength(50)
            .WithMessage("Icon is required and must be less than 50 characters");
    }
}
