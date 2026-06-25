using Api.Common;
using Api.Domain;
using Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.Categories.Create;

public record CreateCategoryRequest(string Name, string HexColor, string Icon);
public record CreateCategoryResponse(Guid Id, string Name, string HexColor, string Icon)
{
    public static CreateCategoryResponse FromEntity(Category c) =>
        new(c.Id, c.Name, c.HexColor, c.Icon);
}

public static class CreateCategory
{
    public static IEndpointRouteBuilder MapCreateCategoryEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/categories", Handle)
            .WithTags("Categories")
            .WithName("CreateCategory")
            .AddEndpointFilter<ValidationFilter<CreateCategoryRequest>>();

        return app;
    }

    private static async Task<IResult> Handle(CreateCategoryRequest request, AppDbContext context, CancellationToken ct)
    {

        var exists = await context.Categories.AnyAsync(c => c.Name.ToLower() == request.Name.ToLower(), ct);

        if (exists)
            return Results.Conflict("Category with this name already exists");

        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            HexColor = request.HexColor,
            Icon = request.Icon,
            IsDefault = false,
            IsArchived = false,
        };

        context.Categories.Add(category);
        await context.SaveChangesAsync(ct);

        return Results.Created($"/api/categories/{category.Id}", CreateCategoryResponse.FromEntity(category));
    }
}


