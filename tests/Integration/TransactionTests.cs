

using System.Net;
using System.Net.Http.Json;



namespace Tests.Integration;

public class TransactionTests(TestWebAppFactory factory) : IClassFixture<TestWebAppFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task GetTransactionById_ReturnsNotFound_WhenIdDoesNotExist()
    {
        var response = await _client.GetAsync($"/api/transactions/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateTransaction_PersistsWithCorrectPln_WhenCurrencyIsPln()
    {
        var name = $"test-{Guid.NewGuid()}";
        var category = new { name, hexColor = "#FF5733", icon = "tag.fill" };

        var categoryResponse = await _client.PostAsJsonAsync("/api/categories", category);

        Assert.Equal(HttpStatusCode.Created, categoryResponse.StatusCode);

        var categoryCreated = await categoryResponse.Content.ReadFromJsonAsync<CategoryDto>();

        var transaction = new { MerchantName = "Żabka", Amount = 100m, Currency = "PLN", Type = "Expense", CategoryId = categoryCreated!.Id };

        var transactionResponse = await _client.PostAsJsonAsync("/api/transactions", transaction);

        Assert.Equal(HttpStatusCode.Created, transactionResponse.StatusCode);

        var transactionCreated = await transactionResponse.Content.ReadFromJsonAsync<TransactionDto>();

        Assert.Equal(100m, transactionCreated!.AmountInPLN);

    }

    [Fact]
    public async Task CreateTransaction_ReturnsBadRequest_WhenCategoryIsArchived()
    {
        var name = $"test-{Guid.NewGuid()}";
        var category = new { name, hexColor = "#FF5733", icon = "tag.fill" };

        var createCat = await _client.PostAsJsonAsync("/api/categories", category);

        Assert.Equal(HttpStatusCode.Created, createCat.StatusCode);

        var categoryCreated = await createCat.Content.ReadFromJsonAsync<CategoryDto>();

        var archivedCategory = await _client.PatchAsync($"/api/categories/{categoryCreated!.Id}/archive", null);

        Assert.Equal(HttpStatusCode.OK, archivedCategory.StatusCode);


        var transaction = new { MerchantName = "Żabka", Amount = 100m, Currency = "PLN", Type = "Expense", CategoryId = categoryCreated!.Id };

        var createTransaction = await _client.PostAsJsonAsync("/api/transactions", transaction);

        Assert.Equal(HttpStatusCode.BadRequest, createTransaction.StatusCode);
    }


    private record TransactionDto(
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
