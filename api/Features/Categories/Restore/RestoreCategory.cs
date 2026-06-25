using Api.Domain;
using Api.Infrastructure;

namespace Api.Features.Categories.Restore;

public record RestoreCategoryResponse(Guid Id, string Name, string HexColor, string Icon, bool IsDefault, bool IsArchived)
{
    public static RestoreCategoryResponse FromEntity(Category c) =>
        new(c.Id, c.Name, c.HexColor, c.Icon, c.IsDefault, c.IsArchived);
}

public static class RestoreCategory
{
    public static IEndpointRouteBuilder MapRestoreCategoryEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPatch("/api/categories/{id}/restore", Handle)
            .WithTags("Categories")
            .WithName("RestoreCategory");

        return app;
    }

    private static async Task<IResult> Handle(
        Guid id,
        AppDbContext context,
        CancellationToken ct)
    {
        var category = await context.Categories.FindAsync([id], ct);
        if (category is null)
            return Results.NotFound("Category not found");


        if (!category.IsArchived)
            return Results.Ok(RestoreCategoryResponse.FromEntity(category));

        category.IsArchived = false;
        await context.SaveChangesAsync(ct);
        return Results.Ok(RestoreCategoryResponse.FromEntity(category));

    }
}
