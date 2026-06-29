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
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to)
    {
        var query = context.Transactions
            .WhereIf(from is not null, t => t.Date >= from!.Value)
            .WhereIf(to is not null, t => t.Date <= to!.Value);

        var totalIncome = await query
            .Where(t => t.Type == TransactionType.Income)
            .SumAsync(t => t.AmountInPLN, ct);

        var TotalExpense = await query
            .Where(t => t.Type == TransactionType.Expense)
            .SumAsync(t => t.AmountInPLN, ct);

        return Results.Ok(new BalanceSummaryResponse(
            totalIncome,
            TotalExpense,
            totalIncome - TotalExpense));
    }

}
