
using Api.Infrastructure;

namespace Api.Features.Receipts.UploadReceipt;

public static class UploadReceipt
{
    public static IEndpointRouteBuilder MapUploadReceiptEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/receipts", Handle)
            .WithTags("Receipts")
            .WithName("UploadReceipt")
            .DisableAntiforgery();

        return app;
    }

    private static async Task<IResult> Handle(
        IFormFile file,
        ReceiptStorage storage,
        CancellationToken ct)
    {
        if (file.Length == 0)
            return Results.BadRequest("File is empty.");
        if (file.Length > 5 * 1024 * 1024)
            return Results.BadRequest("File too large (max 5mb)");
        var allowed = new[] { "image/jpeg", "image/png" };
        if (!allowed.Contains(file.ContentType))
            return Results.BadRequest("Only JPEG and PNG allowed");

        await using var stream = file.OpenReadStream();
        var key = await storage.UploadAsync(stream, file.ContentType, ct);

        return Results.Ok(new { key });

    }


}
