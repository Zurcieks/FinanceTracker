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
        [FromQuery] string? from,
        [FromQuery] string? to)
    {
        if (!DateParsing.TryParseOptional(from, out var fromDate))
            return Results.BadRequest("Invalid 'from' date format");
        if (!DateParsing.TryParseOptional(to, out var toDate))
            return Results.BadRequest("Invalid 'to' date format");

        var rows = await context.Transactions
            .Where(t => t.Type == TransactionType.Expense)
            .WhereIf(fromDate is not null, t => t.Date >= fromDate!.Value)
            .WhereIf(toDate is not null, t => t.Date <= toDate!.Value)
            .GroupBy(t => new { t.CategoryId, t.Category.Name, t.Category.HexColor })
            .Select(g => new
            {
                g.Key.CategoryId,
                g.Key.Name,
                g.Key.HexColor,
                Total = g.Sum(t => t.AmountInPLN)
            })
        .OrderByDescending(x => x.Total)
        .ToListAsync(ct);

        var result = rows
            .Select(r => new CategorySpendingResponse(r.CategoryId, r.Name, r.HexColor, r.Total))
        .ToList();

        return Results.Ok(result);
    }
}


