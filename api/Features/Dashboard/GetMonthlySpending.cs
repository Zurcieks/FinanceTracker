using Api.Common.Extensions;
using Api.Domain;
using Api.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APi.Features.Dashboard;

public record MonthlySpendingResponse(
    int Year,
    int Month,
    decimal Total);

public static class MonthlySpending
{
    public static IEndpointRouteBuilder MapGetMonthlySpending(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/dashboard/monthly", Handle)
            .WithTags("Dashboard")
            .WithName("GetMonthlySpending");

        return app;
    }

    private static async Task<IResult> Handle(
        AppDbContext context,
        CancellationToken ct,
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to)
    {
        var rows = await context.Transactions
            .Where(t => t.Type == TransactionType.Expense)
            .WhereIf(from is not null, t => t.Date >= from!.Value)
            .WhereIf(to is not null, t => t.Date <= from!.Value)
            .Select(t => new { t.Date, t.AmountInPLN })
            .ToListAsync(ct);

        var result = rows
            .GroupBy(r => new { r.Date.Year, r.Date.Month })
            .Select(g => new MonthlySpendingResponse(
                g.Key.Year,
                g.Key.Month,
                g.Sum(r => r.AmountInPLN)))
            .OrderBy(x => x.Year).ThenBy(x => x.Month)
            .ToList();

        return Results.Ok(result);
    }
}


