using Api.Domain;
using Api.Infrastructure;

namespace Api.Features.Categories.Archive;

public record ArchiveCategoryResponse(Guid Id, string Name, string HexColor, string Icon, bool IsDefault, bool IsArchived)
{
    public static ArchiveCategoryResponse FromEntity(Category c) =>
        new(c.Id, c.Name, c.HexColor, c.Icon, c.IsDefault, c.IsArchived);
}

public static class ArchiveCategory
{
    public static IEndpointRouteBuilder MapArchiveCategoryEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPatch("/api/categories/{id}/archive", Handle)
            .WithTags("Categories")
            .WithName("ArchiveCategory");

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

        if (category.IsDefault)
            return Results.BadRequest("Default category cannot be archived");

        if (category.IsArchived)
            return Results.Ok(ArchiveCategoryResponse.FromEntity(category));

        category.IsArchived = true;
        await context.SaveChangesAsync(ct);
        return Results.Ok(ArchiveCategoryResponse.FromEntity(category));

    }
}
