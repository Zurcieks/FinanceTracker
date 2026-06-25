using Api.Common.Extensions;
using Api.Domain;
using Api.Infrastructure;
using Microsoft.EntityFrameworkCore;
namespace Api.Features.Receipts.ScanReceipt;

public record ScanReceiptResponse(
    string? MerchantName,
    decimal? Amount,
    TransactionCurrency? Currency,
    DateOnly? Date,
    TransactionType? Type,
    Guid? SuggestedCategoryId);

public static class ScanReceipt
{
    public static IEndpointRouteBuilder MapScanReceiptEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/receipts/scan", Handle)
           .WithTags("Receipts")
           .WithName("ScanReceipt")
           .DisableAntiforgery();
        return app;
    }

    private static async Task<IResult> Handle(
        IFormFile file,
        AppDbContext db,
        ReceiptScanner scanner,
        CancellationToken ct)
    {
        if (file.Length == 0) return Results.BadRequest("File is empty");
        if (file.Length > 5 * 1024 * 1024) return Results.BadRequest("File too large");
        var allowed = new[] { "image/jpeg", "image/png" };
        if (!allowed.Contains(file.ContentType)) return Results.BadRequest("Only JPEG/PNG");

        var categories = await db.Categories
            .Where(c => !c.IsArchived)
            .Select(c => new { c.Id, c.Name })
            .ToListAsync(ct);

        var categoriesContext = string.Join("\n",
            categories.Select(c => $"{c.Id}: {c.Name}"));

        await using var stream = file.OpenReadStream();
        var scanned = await scanner.ScanAsync(stream, file.ContentType, categoriesContext, ct);

        if (scanned is null)
            return Results.Problem("Could not scan receipt", statusCode: 502);

        var validIds = categories.Select(c => c.Id).ToHashSet();

        var response = new ScanReceiptResponse(
            MerchantName: scanned.MerchantName,
            Amount: scanned.Amount,
            Currency: EnumParsing.ParseOrNull<TransactionCurrency>(scanned.Currency),
            Date: scanned.Date,
            Type: EnumParsing.ParseOrNull<TransactionType>(scanned.Type),
            SuggestedCategoryId: scanned.SuggestedCategoryId is { } id && validIds.Contains(id)
                ? id
                : null);

        return Results.Ok(response);
    }


}
