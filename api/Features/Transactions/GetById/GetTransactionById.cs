using Api.Domain;
using Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.Transactions.GetById;

public record TransactionDetailResponse(
    Guid Id,
    string MerchantName,
    string? Description,
    decimal Amount,
    decimal AmountInPLN,
    TransactionCurrency Currency,
    TransactionType Type,
    DateOnly Date,
    DateTime CreatedAt,
    string? ReceiptUrl,
    CategorySummary Category
);

public record CategorySummary(Guid Id, string Name, string Icon, string HexColor);



public static class GetTransactionById
{
    public static IEndpointRouteBuilder MapGetTransactionByIdEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/transactions/{id}", Handle)
            .WithTags("Transactions")
            .WithName("GetTransactionById");

        return app;
    }

    private static async Task<IResult> Handle(Guid id, AppDbContext context, ReceiptStorage storage, CancellationToken ct)
    {
        var t = await context.Transactions
            .AsNoTracking()
            .Include(t => t.Category)
            .FirstOrDefaultAsync(t => t.Id == id, ct);

        if (t is null)
            return Results.NotFound("Transaction not found");

        var receiptUrl = t.ReceiptKey is null ? null : storage.GetUrl(t.ReceiptKey);

        return Results.Ok(new TransactionDetailResponse(
        t.Id, t.MerchantName, t.Description, t.Amount, t.AmountInPLN,
        t.Currency, t.Type, t.Date, t.CreatedAt, receiptUrl,
        new CategorySummary(t.Category.Id, t.Category.Name, t.Category.Icon, t.Category.HexColor)));
    }
}
