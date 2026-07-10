using Api.Common.Extensions;
using Api.Domain;
using Api.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.Dashboard;

public record MonthlySpendingResponse(
    int Year,
    int Month,
    decimal Total);

public static class GetMonthlySpending
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
            .GroupBy(t => new { t.Date.Year, t.Date.Month })
            .Select(g => new
            {
                g.Key.Year,
                g.Key.Month,
                Total = g.Sum(t => t.AmountInPLN)
            })
            .OrderBy(x => x.Year).ThenBy(x => x.Month)
            .ToListAsync(ct);

        var result = rows
            .Select(r => new MonthlySpendingResponse(r.Year, r.Month, r.Total)) // EF cant translate agreggate in constructor
        .ToList();

        return Results.Ok(result);
    }
}


