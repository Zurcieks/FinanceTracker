using Api.Common.Extensions;
using Api.Domain;
using Api.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.Dashboard;

public record BalanceSummaryResponse(
    decimal TotalIncome,
    decimal TotalExpense,
    decimal Balance
);

public static class GetBalanceSummary
{
    public static IEndpointRouteBuilder MapGetBalanceSummary(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/dashboard/balance", Handle)
            .WithTags("Dashboard")
            .WithName("GetBalanceSummary");

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

        var query = context.Transactions
            .WhereIf(fromDate is not null, t => t.Date >= fromDate!.Value)
            .WhereIf(toDate is not null, t => t.Date <= toDate!.Value);

        var totalIncome = await query
            .Where(t => t.Type == TransactionType.Income)
            .SumAsync(t => t.AmountInPLN, ct);

        var totalExpense = await query
            .Where(t => t.Type == TransactionType.Expense)
            .SumAsync(t => t.AmountInPLN, ct);

        return Results.Ok(new BalanceSummaryResponse(
            totalIncome,
            totalExpense,
            totalIncome - totalExpense));
    }

}
