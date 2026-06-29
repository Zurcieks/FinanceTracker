using Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace Api.Infrastructure.Seeding;

public static class TransactionSeeder
{
    private const decimal EurRate = 4.30m;
    private const int Year = 2026;
    private const int Month = 6;

    public static async Task SeedAsync(AppDbContext db, CancellationToken ct = default)
    {
        if (await db.Transactions.AnyAsync(ct))
        {
            return;
        }

        var categoryIdsByName = await db.Categories
            .ToDictionaryAsync(c => c.Name, c => c.Id, ct);

        var transactions = Items
            .Where(i => categoryIdsByName.ContainsKey(i.Category))
            .Select(i =>
            {
                var date = new DateOnly(Year, Month, i.Day);
                var amountInPln = i.Currency == TransactionCurrency.EUR
                    ? Math.Round(i.Amount * EurRate, 2)
                    : i.Amount;

                return new Transaction
                {
                    Id = Guid.NewGuid(),
                    MerchantName = i.Merchant,
                    Description = i.Description,
                    Amount = i.Amount,
                    AmountInPLN = amountInPln,
                    Currency = i.Currency,
                    Type = i.Type,
                    ReceiptKey = null,
                    Date = date,
                    CreatedAt = new DateTime(Year, Month, i.Day, 12, 0, 0, DateTimeKind.Utc),
                    CategoryId = categoryIdsByName[i.Category],
                };
            })
            .ToList();

        db.Transactions.AddRange(transactions);
        await db.SaveChangesAsync(ct);
    }

    private record SeedItem(
        string Merchant,
        decimal Amount,
        TransactionCurrency Currency,
        TransactionType Type,
        int Day,
        string Category,
        string? Description = null);

    private static readonly SeedItem[] Items =
    [
        new("Netflix", 43.00m, TransactionCurrency.PLN, TransactionType.Expense, 1, "Subskrypcje"),
        new("Spotify", 23.99m, TransactionCurrency.PLN, TransactionType.Expense, 1, "Subskrypcje"),
        new("iCloud+", 14.99m, TransactionCurrency.PLN, TransactionType.Expense, 1, "Subskrypcje"),

        new("Biedronka", 87.45m, TransactionCurrency.PLN, TransactionType.Expense, 2, "Jedzenie"),

        new("McDonald's", 32.50m, TransactionCurrency.PLN, TransactionType.Expense, 3, "Restauracje"),
        new("Orlen", 280.00m, TransactionCurrency.PLN, TransactionType.Expense, 3, "Transport", "Tankowanie"),

        new("Zalando", 219.99m, TransactionCurrency.PLN, TransactionType.Expense, 5, "Zakupy"),
        new("Żabka", 12.50m, TransactionCurrency.PLN, TransactionType.Expense, 5, "Jedzenie"),

        new("Lidl", 124.30m, TransactionCurrency.PLN, TransactionType.Expense, 6, "Jedzenie"),

        new("Apteka DOZ", 54.30m, TransactionCurrency.PLN, TransactionType.Expense, 7, "Zdrowie"),

        new("Bolt", 23.40m, TransactionCurrency.PLN, TransactionType.Expense, 8, "Transport"),

        new("Czynsz", 1800.00m, TransactionCurrency.PLN, TransactionType.Expense, 10, "Mieszkanie i rachunki"),
        new("Pracodawca Sp. z o.o.", 8500.00m, TransactionCurrency.PLN, TransactionType.Income, 10, "Inne", "Wynagrodzenie"),

        new("Pasibus", 45.00m, TransactionCurrency.PLN, TransactionType.Expense, 11, "Restauracje"),

        new("MPK Wrocław", 120.00m, TransactionCurrency.PLN, TransactionType.Expense, 12, "Transport", "Bilet miesięczny"),
        new("Booking.com", 120.00m, TransactionCurrency.EUR, TransactionType.Expense, 12, "Podróże", "Nocleg"),

        new("Cinema City", 46.00m, TransactionCurrency.PLN, TransactionType.Expense, 13, "Rozrywka"),

        new("Auchan", 203.77m, TransactionCurrency.PLN, TransactionType.Expense, 14, "Jedzenie"),

        new("Tauron", 145.20m, TransactionCurrency.PLN, TransactionType.Expense, 15, "Mieszkanie i rachunki", "Prąd"),
        new("PGNiG", 98.60m, TransactionCurrency.PLN, TransactionType.Expense, 15, "Mieszkanie i rachunki", "Gaz"),
        new("Vectra", 79.00m, TransactionCurrency.PLN, TransactionType.Expense, 15, "Mieszkanie i rachunki", "Internet"),

        new("Media Expert", 349.00m, TransactionCurrency.PLN, TransactionType.Expense, 17, "Zakupy"),

        new("KFC", 39.90m, TransactionCurrency.PLN, TransactionType.Expense, 18, "Restauracje"),
        new("Freelance - projekt", 1500.00m, TransactionCurrency.PLN, TransactionType.Income, 18, "Inne"),

        new("Medicover", 160.00m, TransactionCurrency.PLN, TransactionType.Expense, 19, "Zdrowie", "Wizyta prywatna"),

        new("Biedronka", 66.10m, TransactionCurrency.PLN, TransactionType.Expense, 20, "Jedzenie"),

        new("Steam", 89.99m, TransactionCurrency.PLN, TransactionType.Expense, 21, "Rozrywka", "Gra"),

        new("BP", 260.50m, TransactionCurrency.PLN, TransactionType.Expense, 22, "Transport", "Tankowanie"),

        new("Udemy", 59.99m, TransactionCurrency.PLN, TransactionType.Expense, 23, "Edukacja", "Kurs online"),

        new("Bankomat", 200.00m, TransactionCurrency.PLN, TransactionType.Expense, 24, "Inne", "Wypłata gotówki"),

        new("Sphinx", 88.00m, TransactionCurrency.PLN, TransactionType.Expense, 25, "Restauracje"),
        new("Żabka", 18.99m, TransactionCurrency.PLN, TransactionType.Expense, 25, "Jedzenie"),

        new("Empik", 74.80m, TransactionCurrency.PLN, TransactionType.Expense, 26, "Edukacja", "Książki"),

        new("Carrefour", 91.25m, TransactionCurrency.PLN, TransactionType.Expense, 27, "Jedzenie"),

        new("IKEA", 142.50m, TransactionCurrency.PLN, TransactionType.Expense, 28, "Zakupy"),

        new("Ryanair", 89.99m, TransactionCurrency.EUR, TransactionType.Expense, 29, "Podróże", "Lot"),

        new("Lidl", 110.20m, TransactionCurrency.PLN, TransactionType.Expense, 30, "Jedzenie"),
        new("H&M", 159.99m, TransactionCurrency.PLN, TransactionType.Expense, 30, "Zakupy"),
    ];
}
