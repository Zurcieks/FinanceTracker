using Api.Common;
using Api.Domain;
using Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.Categories.Update;


public record UpdateCategoryRequest(string Name, string HexColor, string Icon);
public record UpdateCategoryResponse(string Name, string HexColor, string Icon)
{
    public static UpdateCategoryResponse FromEntity(Category c) =>
        new(c.Name, c.HexColor, c.Icon);
}

public static class UpdateCategory
{
    public static IEndpointRouteBuilder MapUpdateCategoryEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPatch("/api/categories/{id}", Handle)
            .WithTags("Categories")
            .WithName("UpdateCategory")
            .AddEndpointFilter<ValidationFilter<UpdateCategoryRequest>>();

        return app;
    }

    private static async Task<IResult> Handle(
        Guid id,
        AppDbContext context,
        UpdateCategoryRequest request,
        CancellationToken ct)
    {
        var category = await context.Categories.FindAsync([id], ct);

        if (category is null)
            return Results.NotFound("Category not found");

        if (category.IsDefault)
            return Results.BadRequest("Default category cannot be edited");

        var nameTaken = await context.Categories
            .AnyAsync(c => c.Id != id && c.Name.ToLower() == request.Name.ToLower(), ct);

        if (nameTaken)
            return Results.Conflict("Category with this name already exists");

        category.Name = request.Name;
        category.HexColor = request.HexColor;
        category.Icon = request.Icon;

        await context.SaveChangesAsync(ct);

        return Results.Ok(UpdateCategoryResponse.FromEntity(category));




    }

}
