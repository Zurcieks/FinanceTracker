using Api.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.Categories.Get;

public record GetCategoriesResponse(Guid Id, string Name, string HexColor, string Icon, bool IsDefault, bool IsArchived);
public static class GetCategories
{
    public static IEndpointRouteBuilder MapGetCategoriesEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/categories", Handle)
            .WithTags("Categories")
            .WithName("GetCategories");

        return app;
    }


    private static async Task<IResult> Handle(
        AppDbContext context,
        CancellationToken ct,
        [FromQuery] bool includeArchived = false)
    {
        var query = context.Categories.AsNoTracking().AsQueryable();

        if (!includeArchived)
        {
            query = query.Where(c => !c.IsArchived);
        }

        var categories = await query
            .OrderBy(c => c.Name)
            .Select(c => new GetCategoriesResponse(
                c.Id, c.Name, c.HexColor, c.Icon, c.IsDefault, c.IsArchived
            ))
            .ToListAsync(ct);

        return Results.Ok(categories);

    }
}
