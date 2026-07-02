using Api.Common;
using Api.Domain;
using Api.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Api.Features.Transactions.Update;

public record UpdateTransactionRequest(
    string MerchantName,
    string? Description,
    decimal Amount,
    TransactionCurrency Currency,
    TransactionType Type,
    DateOnly? Date,
    Guid CategoryId,
    string? ReceiptKey,
    bool RemoveReceipt = false);

public record UpdateTransactionResponse(
    Guid Id,
    string MerchantName,
    string? Description,
    decimal Amount,
    decimal AmountInPLN,
    TransactionCurrency Currency,
    TransactionType Type,
    string? ReceiptKey,
    DateOnly Date,
    DateTime CreatedAt,
    Guid CategoryId)
{
    public static UpdateTransactionResponse FromEntity(Transaction t) =>
        new(t.Id, t.MerchantName, t.Description, t.Amount, t.AmountInPLN, t.Currency, t.Type, t.ReceiptKey, t.Date, t.CreatedAt, t.CategoryId);
};



public static class UpdateTransaction
{

    public static IEndpointRouteBuilder MapUpdateTransactionEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPatch("/api/transactions/{id}", Handle)
            .WithTags("Transactions")
            .WithName("UpdateTransaction")
            .AddEndpointFilter<ValidationFilter<UpdateTransactionRequest>>();

        return app;
    }

    private static async Task<IResult> Handle(
        Guid id,
        UpdateTransactionRequest request,
        AppDbContext context, CurrencyConverter
        converter,
        ReceiptStorage storage,
        ILogger<UpdateTransactionRequest> logger,
        CancellationToken ct)
    {
        var transaction = await context.Transactions.FindAsync([id], ct);
        if (transaction is null)
            return Results.NotFound("Transaction not found");

        var category = await context.Categories
            .FirstOrDefaultAsync(c => c.Id == request.CategoryId, ct);
        if (category is null)
            return Results.BadRequest("Category not found");
        if (category.IsArchived)
            return Results.BadRequest("Cannot assign transaction to archived category");

        var amountInPLN = await converter.ToPlnAsync(request.Amount, request.Currency, ct);

        if (amountInPLN is null)
            return Results.Problem("Failed to fetch euro rate from NBP", statusCode: 502);

        if (request.ReceiptKey is not null
            && request.ReceiptKey != transaction.ReceiptKey
            && !await storage.ExistsAsync(request.ReceiptKey, ct))
        {
            return Results.BadRequest("Receipt not found.");
        }

        string? keyToDelete = null;

        if (request.ReceiptKey is not null && request.ReceiptKey != transaction.ReceiptKey)
        {
            keyToDelete = transaction.ReceiptKey;
            transaction.ReceiptKey = request.ReceiptKey;
        }
        else if (request.RemoveReceipt && transaction.ReceiptKey is not null)
        {
            keyToDelete = transaction.ReceiptKey;
            transaction.ReceiptKey = null;
        }

        var warsaw = TimeZoneInfo.FindSystemTimeZoneById("Europe/Warsaw");
        var today = DateOnly.FromDateTime(
            TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, warsaw));
        var date = request.Date ?? today;

        transaction.MerchantName = request.MerchantName;
        transaction.Description = request.Description;
        transaction.Amount = request.Amount;
        transaction.AmountInPLN = amountInPLN.Value;
        transaction.Currency = request.Currency;
        transaction.Type = request.Type;
        transaction.Date = date;
        transaction.CategoryId = request.CategoryId;

        await context.SaveChangesAsync(ct);


        if (keyToDelete is not null)
        {
            try
            {
                await storage.DeleteAsync(keyToDelete, ct);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex,
                "Failed to delete old receipt after updating transaction {TransactionId}. Orphaned file left in storage.",
                transaction.Id);
            }
        }
        return Results.Ok(UpdateTransactionResponse.FromEntity(transaction));


    }


}
