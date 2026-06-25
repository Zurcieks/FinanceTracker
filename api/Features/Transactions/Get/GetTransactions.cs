using Api.Domain;
using Api.Infrastructure;
using Api.Common.Extensions;
using Api.Common.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.Transactions.Get;

public record GetTransactionResponse(
    Guid Id, string MerchantName, string? Description,
    decimal Amount, decimal AmountInPLN, TransactionCurrency Currency,
    TransactionType Type, DateOnly Date, CategorySummary Summary);

public record CategorySummary(Guid Id, string Name, string Icon, string HexColor);

public static class GetTransactions
{
    public static IEndpointRouteBuilder MapGetTransactionEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/transactions", Handle)
            .WithTags("Transactions")
            .WithName("GetTransactions");

        return app;
    }

    private static async Task<IResult> Handle(
        AppDbContext context,
        CancellationToken ct,
        [FromQuery] Guid? categoryId,
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {

        page = page < 1 ? 1 : page;
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = context.Transactions.AsNoTracking().AsQueryable();

        query = query
            .WhereIf(categoryId is not null, t => t.CategoryId == categoryId)
                .WhereIf(from is not null, t => t.Date >= from!.Value)
                .WhereIf(to is not null, t => t.Date <= to!.Value)
                .WhereIf(!string.IsNullOrWhiteSpace(search), t =>
                EF.Functions.ILike(t.MerchantName, $"%{search}%") ||
                (t.Description != null && EF.Functions.ILike(t.Description, $"%{search}%")));

        var totalCount = await query.CountAsync(ct);

        var transactions = await query
            .OrderByDescending(t => t.Date)
            .ThenByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new GetTransactionResponse(
                t.Id,
                t.MerchantName,
                t.Description,
                t.Amount,
                t.AmountInPLN,
                t.Currency,
                t.Type,
                t.Date,
                new CategorySummary(
                    t.Category.Id,
                    t.Category.Name,
                    t.Category.Icon,
                    t.Category.HexColor
                )
            )).ToListAsync(ct);

        return Results.Ok(new PagedResult<GetTransactionResponse>(transactions, page, pageSize, totalCount));

    }
}
