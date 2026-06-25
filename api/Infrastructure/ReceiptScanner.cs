using System.Text.Json;
using OpenAI.Chat;

namespace Api.Infrastructure;


public class ReceiptScanner(IConfiguration config)
{
    public async Task<ScannedReceipt?> ScanAsync(
        Stream image, string contentType, string categoriesContext, CancellationToken ct)
    {
        var client = new ChatClient("gpt-5.4-mini", config["OpenAI:ApiKey"]);

        using var ms = new MemoryStream();
        await image.CopyToAsync(ms, ct);
        var imageBytes = BinaryData.FromBytes(ms.ToArray());

        var prompt = $$"""
            Jesteś asystentem wyciągającym dane z paragonów.
            Przeanalizuj obraz paragonu i zwróć WYŁĄCZNIE JSON o strukturze:
            {
              "merchantName": string | null,
              "amount": number | null,
              "currency": "PLN" | "EUR" | null,
              "date": "YYYY-MM-DD" | null,
              "type": "Expense" | "Income" | null,
              "suggestedCategoryId": string | null
            }
            Jeśli jakiegoś pola nie da się odczytać, ustaw null.
            Dla suggestedCategoryId wybierz NAJLEPIEJ pasującą kategorię
            z poniższej listy (zwróć jej Id) albo null:
            {{categoriesContext}}
            Nie dodawaj żadnego tekstu poza JSON.
            """;

        var messages = new List<ChatMessage>
        {
            new UserChatMessage(
                ChatMessageContentPart.CreateTextPart(prompt),
                ChatMessageContentPart.CreateImagePart(imageBytes, contentType)
            )
        };

        var options = new ChatCompletionOptions
        {
            Temperature = 0f,
            ResponseFormat = ChatResponseFormat.CreateJsonObjectFormat()
        };

        var completion = await client.CompleteChatAsync(messages, options, ct);
        var json = completion.Value.Content[0].Text;

        return JsonSerializer.Deserialize<ScannedReceipt>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }
}

public record ScannedReceipt(
    string? MerchantName, decimal? Amount, string? Currency,
    DateOnly? Date, string? Type, Guid? SuggestedCategoryId);
