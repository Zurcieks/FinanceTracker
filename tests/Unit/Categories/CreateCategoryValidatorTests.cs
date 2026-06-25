

using Api.Features.Categories.Create;
using FluentValidation.TestHelper;

namespace Tests.Unit.Categories;

public class CreateCategoryValidatorTests
{
    private readonly CreateCategoryValidator _validator = new();

    [Fact]
    public void Should_HaveError_When_NameIsEmpty()
    {
        // Arrange
        var request = new CreateCategoryRequest("", "FF5733", "tag.fill");

        // Act
        var result = _validator.TestValidate(request);

        // Result
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_NotHaveError_When_RequestIsValid()
    {
        var request = new CreateCategoryRequest("Jedzenie", "#FF5733", "tag.fill");

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("FF5733")] // #
    [InlineData("FF57")] // too short
    [InlineData("#GGGGGG")] // not hex
    public void Should_HaveError_When_HexColorInvalid(string badColor)
    {
        var request = new CreateCategoryRequest("Jedzenie", badColor, "tag.fill");

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.HexColor);
    }
}
