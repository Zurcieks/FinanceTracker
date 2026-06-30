using Api.Common.Extensions;
using Api.Domain;
using Api.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.Dashboard;


public record CategorySpendingResponse(
    Guid CategoryId,
    string CategoryName,
    string HexColor,
    decimal Total);

public static class GetCategorySpending
{
    public static IEndpointRouteBuilder MapGetCategorySpending(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/dashboard/category", Handle)
            .WithTags("Dashboard")
            .WithName("GetCategorySpending");
        return app;
    }

    private static async Task<IResult> Handle(
        AppDbContext context,
        CancellationToken ct,
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to)
    {
        var spending = await context.Transactions
            .Where(t => t.Type == TransactionType.Expense)
            .WhereIf(from is not null, t => t.Date >= from!.Value)
            .WhereIf(to is not null, t => t.Date <= to!.Value)
            .GroupBy(t => t.CategoryId)
            .Select(g => new
            {
                CategoryId = g.Key,
                Total = g.Sum(t => t.AmountInPLN)
            })
        .ToListAsync(ct);

        var categoryIds = spending.Select(s => s.CategoryId).ToList();
        var categories = await context.Categories
            .Where(c => categoryIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, c => new { c.Name, c.HexColor }, ct);

        var result = spending
            .Select(s => new CategorySpendingResponse(
                s.CategoryId,
                categories[s.CategoryId].Name,
                categories[s.CategoryId].HexColor,
                s.Total))
            .OrderByDescending(x => x.Total)
            .ToList();

        return Results.Ok(result);
    }
}


