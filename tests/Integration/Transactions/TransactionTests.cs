using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
namespace Tests.Integration;

public class TransactionTests(TestWebAppFactory factory) : IClassFixture<TestWebAppFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private async Task<Guid> CreateCategoryAsync()
    {
        var category = new { name = $"test-{Guid.NewGuid()}", hexColor = "#FF5733", icon = "tag.fill" };
        var response = await _client.PostAsJsonAsync("/api/categories", category);
        response.EnsureSuccessStatusCode();
        var created = await response.Content.ReadFromJsonAsync<CategoryDto>();
        return created!.Id;
    }

    private async Task<HttpResponseMessage> PostTransactionAsync(Guid categoryId, string merchant, decimal amount)
    {
        var transaction = new
        {
            MerchantName = merchant,
            Amount = amount,
            Currency = "PLN",
            Type = "Expense",
            CategoryId = categoryId
        };
        return await _client.PostAsJsonAsync("/api/transactions", transaction);
    }

    private async Task<string> UploadReceiptAsync()
    {
        using var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent([0xFF, 0xD8, 0xFF, 0xE0]);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        content.Add(fileContent, "file", "receipt.jpg");

        var response = await _client.PostAsync("/api/receipts", content);
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        return body!["key"];
    }

    private async Task<bool> ReceiptExistsAsync(string key)
    {
        var files = await _client.GetFromJsonAsync<List<DebugFileDto>>("/api/debug/receipts");
        return files!.Any(f => f.Key == key);
    }

    [Fact]
    public async Task GetTransactionById_ReturnsNotFound_WhenIdDoesNotExist()
    {
        var response = await _client.GetAsync($"/api/transactions/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateTransaction_PersistsWithCorrectPln_WhenCurrencyIsPln()
    {
        var categoryId = await CreateCategoryAsync();

        var response = await PostTransactionAsync(categoryId, "Żabka", 100m);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var created = await response.Content.ReadFromJsonAsync<TransactionDto>();
        Assert.Equal(100m, created!.AmountInPLN);
    }

    [Fact]
    public async Task CreateTransaction_ReturnsBadRequest_WhenCategoryIsArchived()
    {
        var categoryId = await CreateCategoryAsync();

        var archive = await _client.PatchAsync($"/api/categories/{categoryId}/archive", null);
        Assert.Equal(HttpStatusCode.OK, archive.StatusCode);

        var response = await PostTransactionAsync(categoryId, "Żabka", 100m);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateTransaction_PersistsChanges_WhenRequestIsValid()
    {
        var categoryId = await CreateCategoryAsync();
        var createResponse = await PostTransactionAsync(categoryId, "Żabka", 100m);
        var created = await createResponse.Content.ReadFromJsonAsync<TransactionDto>();

        var update = new
        {
            MerchantName = "Biedronka",
            Amount = 250m,
            Currency = "PLN",
            Type = "Expense",
            CategoryId = categoryId
        };
        var response = await _client.PatchAsJsonAsync($"/api/transactions/{created!.Id}", update);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var updated = await response.Content.ReadFromJsonAsync<TransactionDto>();
        Assert.Equal("Biedronka", updated!.MerchantName);
        Assert.Equal(250m, updated.Amount);
        Assert.Equal(250m, updated.AmountInPLN);
    }

    [Fact]
    public async Task UpdateTransaction_ReturnsNotFound_WhenIdDoesNotExist()
    {
        var update = new
        {
            MerchantName = "Żabka",
            Amount = 100m,
            Currency = "PLN",
            Type = "Expense",
            Date = (string?)null,
            CategoryId = Guid.NewGuid()
        };
        var response = await _client.PatchAsJsonAsync($"/api/transactions/{Guid.NewGuid()}", update);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetTransactions_DoesNotFail_WhenDateParamsAreEmpty()
    {
        var response = await _client.GetAsync("/api/transactions?from=&to=");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetTransactions_ReturnsBadRequest_WhenDateFormatIsInvalid()
    {
        var response = await _client.GetAsync("/api/transactions?from=not-a-date");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task DeleteTransaction_RemovesReceiptFromStorage()
    {
        var categoryId = await CreateCategoryAsync();
        var key = await UploadReceiptAsync();

        var create = await _client.PostAsJsonAsync("/api/transactions", new
        {
            MerchantName = "Żabka",
            Amount = 10m,
            Currency = "PLN",
            Type = "Expense",
            CategoryId = categoryId,
            ReceiptKey = key
        });
        create.EnsureSuccessStatusCode();
        var created = await create.Content.ReadFromJsonAsync<TransactionDto>();

        var delete = await _client.DeleteAsync($"/api/transactions/{created!.Id}");
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);

        Assert.False(await ReceiptExistsAsync(key));

        var getAfterDelete = await _client.GetAsync($"/api/transactions/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getAfterDelete.StatusCode);
    }

    [Fact]
    public async Task UpdateTransaction_ReplacingReceipt_DeletesOldFileFromStorage()
    {
        var categoryId = await CreateCategoryAsync();
        var oldKey = await UploadReceiptAsync();

        var create = await PostTransactionAsync(categoryId, "Żabka", 100m);
        var created = await create.Content.ReadFromJsonAsync<TransactionDto>();

        var attach = await _client.PatchAsJsonAsync($"/api/transactions/{created!.Id}", new
        {
            MerchantName = "Żabka",
            Amount = 100m,
            Currency = "PLN",
            Type = "Expense",
            CategoryId = categoryId,
            ReceiptKey = oldKey
        });
        Assert.Equal(HttpStatusCode.OK, attach.StatusCode);

        var newKey = await UploadReceiptAsync();

        var swap = await _client.PatchAsJsonAsync($"/api/transactions/{created.Id}", new
        {
            MerchantName = "Żabka",
            Amount = 100m,
            Currency = "PLN",
            Type = "Expense",
            CategoryId = categoryId,
            ReceiptKey = newKey
        });
        Assert.Equal(HttpStatusCode.OK, swap.StatusCode);

        Assert.False(await ReceiptExistsAsync(oldKey));
        Assert.True(await ReceiptExistsAsync(newKey));
    }

    [Fact]
    public async Task UpdateTransaction_WithRemoveReceipt_ClearsKeyAndDeletesFile()
    {
        var categoryId = await CreateCategoryAsync();
        var key = await UploadReceiptAsync();

        var create = await PostTransactionAsync(categoryId, "Żabka", 100m);
        var created = await create.Content.ReadFromJsonAsync<TransactionDto>();

        var attach = await _client.PatchAsJsonAsync($"/api/transactions/{created!.Id}", new
        {
            MerchantName = "Żabka",
            Amount = 100m,
            Currency = "PLN",
            Type = "Expense",
            CategoryId = categoryId,
            ReceiptKey = key
        });
        Assert.Equal(HttpStatusCode.OK, attach.StatusCode);

        var remove = await _client.PatchAsJsonAsync($"/api/transactions/{created.Id}", new
        {
            MerchantName = "Żabka",
            Amount = 100m,
            Currency = "PLN",
            Type = "Expense",
            CategoryId = categoryId,
            ReceiptKey = (string?)null,
            RemoveReceipt = true
        });
        var removed = await remove.Content.ReadFromJsonAsync<TransactionDto>();

        Assert.Null(removed!.ReceiptKey);
        Assert.False(await ReceiptExistsAsync(key));
    }

    private record DebugFileDto(string Key, DateTime LastModified);

    private record TransactionDto(
        Guid Id,
        string MerchantName,
        string? Description,
        decimal Amount,
        decimal AmountInPLN,
        string Currency,
        string Type,
        string? ReceiptKey,
        DateOnly? Date,
        Guid CategoryId);

    private record CategoryDto(
        Guid Id,
        string Name,
        string HexColor,
        string Icon,
        bool IsDefault,
        bool IsArchived);
}
