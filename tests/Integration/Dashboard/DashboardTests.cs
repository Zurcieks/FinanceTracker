
using System.Net;
using System.Net.Http.Json;

namespace Tests.Integration;

public class DashboardTests(TestWebAppFactory factory) : IClassFixture<TestWebAppFactory>
{

    private readonly HttpClient _client = factory.CreateClient();

    private record CategoryDto(Guid Id);
    private record BalanceSummaryDto(decimal TotalIncome, decimal TotalExpense, decimal Balance);

    private async Task<Guid> CreateCategoryAsync()
    {
        var category = new { name = $"dash-{Guid.NewGuid()}", hexColor = "#FF5733", icon = "tag.fill" };
        var response = await _client.PostAsJsonAsync("/api/categories", category);
        var created = await response.Content.ReadFromJsonAsync<CategoryDto>();
        return created!.Id;
    }

    private async Task CreateExpenseAsync(Guid categoryId, decimal amount, DateOnly date)
    {
        var transaction = new
        {
            MerchantName = "Test",
            Amount = amount,
            Currency = "PLN",
            Type = "Expense",
            CategoryId = categoryId,
            Date = date
        };
        var response = await _client.PostAsJsonAsync("/api/transactions", transaction);
        response.EnsureSuccessStatusCode();
    }

    [Theory]
    [InlineData("/api/dashboard/balance")]
    [InlineData("/api/dashboard/category")]
    [InlineData("/api/dashboard/monthly")]
    public async Task DashboardEndpoins_DoNotFail_WhenDateParamsAreEmpty(string path)
    {
        var response = await _client.GetAsync($"{path}?from=&to=");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [InlineData("/api/dashboard/balance")]
    [InlineData("/api/dashboard/category")]
    [InlineData("/api/dashboard/monthly")]
    public async Task DashboardEndpoints_ReturnBadRequest_WhenDateFormatIsInvalid(string path)
    {
        var response = await _client.GetAsync($"{path}?from=not-a-date");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetBalanceSummary_DateRange_IsInclusiveOnBothEnds()
    {
        var categoryId = await CreateCategoryAsync();

        var from = new DateOnly(2099, 3, 10);
        var to = new DateOnly(2099, 3, 20);

        await CreateExpenseAsync(categoryId, 10m, from.AddDays(-1)); // dzień przed zakresem - nie powinien się liczyć
        await CreateExpenseAsync(categoryId, 100m, from);            // dokładnie granica 'from' - powinien się liczyć
        await CreateExpenseAsync(categoryId, 100m, to);              // dokładnie granica 'to' - powinien się liczyć
        await CreateExpenseAsync(categoryId, 10m, to.AddDays(1));    // dzień po zakresie - nie powinien się liczyć

        var response = await _client.GetAsync($"/api/dashboard/balance?from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}");
        var result = await response.Content.ReadFromJsonAsync<BalanceSummaryDto>();

        Assert.Equal(200m, result!.TotalExpense);
    }

    [Fact]
    public async Task GetBalanceSummary_FromAfterTo_ReturnsZeroTotals_NotError()
    {
        var categoryId = await CreateCategoryAsync();
        await CreateExpenseAsync(categoryId, 100m, new DateOnly(2099, 6, 15));

        var response = await _client.GetAsync("/api/dashboard/balance?from=2099-06-20&to=2099-06-10");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<BalanceSummaryDto>();
        Assert.Equal(0m, result!.TotalExpense);
        Assert.Equal(0m, result.TotalIncome);
    }
}
