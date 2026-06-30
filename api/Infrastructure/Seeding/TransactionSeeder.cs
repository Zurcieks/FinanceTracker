using Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace Api.Infrastructure.Seeding;

public static class TransactionSeeder
{
    private const decimal EurRate = 4.30m;
    private const int Year = 2026;

    private record SeedItem(
        string Merchant,
        decimal Amount,
        TransactionCurrency Currency,
        TransactionType Type,
        int Month,
        int Day,
        string Category,
        string? Description = null);


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
                var date = new DateOnly(Year, i.Month, i.Day);
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
                    CreatedAt = new DateTime(Year, i.Month, i.Day, 12, 0, 0, DateTimeKind.Utc),
                    CategoryId = categoryIdsByName[i.Category],
                };
            })
            .ToList();

        db.Transactions.AddRange(transactions);
        await db.SaveChangesAsync(ct);
    }


    private static readonly SeedItem[] Items =
    [
        new("Netflix", 43.00m, TransactionCurrency.PLN, TransactionType.Expense, 6, 1, "Subskrypcje"),
        new("Spotify", 23.99m, TransactionCurrency.PLN, TransactionType.Expense, 6, 1, "Subskrypcje"),
        new("iCloud+", 14.99m, TransactionCurrency.PLN, TransactionType.Expense, 6, 1, "Subskrypcje"),

        new("Biedronka", 87.45m, TransactionCurrency.PLN, TransactionType.Expense, 6, 2, "Jedzenie"),

        new("McDonald's", 32.50m, TransactionCurrency.PLN, TransactionType.Expense, 6, 3, "Restauracje"),
        new("Orlen", 280.00m, TransactionCurrency.PLN, TransactionType.Expense, 3, 6, "Transport", "Tankowanie"),

        new("Zalando", 219.99m, TransactionCurrency.PLN, TransactionType.Expense, 6, 5, "Zakupy"),
        new("Żabka", 12.50m, TransactionCurrency.PLN, TransactionType.Expense, 6, 5, "Jedzenie"),

        new("Lidl", 124.30m, TransactionCurrency.PLN, TransactionType.Expense, 6, 6, "Jedzenie"),

        new("Apteka DOZ", 54.30m, TransactionCurrency.PLN, TransactionType.Expense, 6, 7, "Zdrowie"),

        new("Bolt", 23.40m, TransactionCurrency.PLN, TransactionType.Expense, 6, 8, "Transport"),

        new("Czynsz", 1800.00m, TransactionCurrency.PLN, TransactionType.Expense, 6, 10, "Mieszkanie i rachunki"),
        new("Pracodawca Sp. z o.o.", 8500.00m, TransactionCurrency.PLN, TransactionType.Income, 6, 10, "Inne", "Wynagrodzenie"),

        new("Pasibus", 45.00m, TransactionCurrency.PLN, TransactionType.Expense, 6, 11, "Restauracje"),

        new("MPK Wrocław", 120.00m, TransactionCurrency.PLN, TransactionType.Expense, 6, 12, "Transport", "Bilet miesięczny"),
        new("Booking.com", 120.00m, TransactionCurrency.EUR, TransactionType.Expense, 6, 12, "Podróże", "Nocleg"),

        new("Cinema City", 46.00m, TransactionCurrency.PLN, TransactionType.Expense, 6, 13, "Rozrywka"),

        new("Auchan", 203.77m, TransactionCurrency.PLN, TransactionType.Expense, 6, 14, "Jedzenie"),

        new("Tauron", 145.20m, TransactionCurrency.PLN, TransactionType.Expense, 6, 15, "Mieszkanie i rachunki", "Prąd"),
        new("PGNiG", 98.60m, TransactionCurrency.PLN, TransactionType.Expense, 6, 15, "Mieszkanie i rachunki", "Gaz"),
        new("Vectra", 79.00m, TransactionCurrency.PLN, TransactionType.Expense, 6,15, "Mieszkanie i rachunki", "Internet"),

        new("Media Expert", 349.00m, TransactionCurrency.PLN, TransactionType.Expense, 6, 17, "Zakupy"),

        new("KFC", 39.90m, TransactionCurrency.PLN, TransactionType.Expense, 6, 18, "Restauracje"),
        new("Freelance - projekt", 1500.00m, TransactionCurrency.PLN, TransactionType.Income, 6, 18, "Inne"),

        new("Medicover", 160.00m, TransactionCurrency.PLN, TransactionType.Expense, 6, 19, "Zdrowie", "Wizyta prywatna"),

        new("Biedronka", 66.10m, TransactionCurrency.PLN, TransactionType.Expense, 6, 20, "Jedzenie"),

        new("Steam", 89.99m, TransactionCurrency.PLN, TransactionType.Expense, 6, 21, "Rozrywka", "Gra"),

        new("BP", 260.50m, TransactionCurrency.PLN, TransactionType.Expense, 6, 22, "Transport", "Tankowanie"),

        new("Udemy", 59.99m, TransactionCurrency.PLN, TransactionType.Expense, 6, 23, "Edukacja", "Kurs online"),

        new("Bankomat", 200.00m, TransactionCurrency.PLN, TransactionType.Expense, 6, 24, "Inne", "Wypłata gotówki"),

        new("Sphinx", 88.00m, TransactionCurrency.PLN, TransactionType.Expense, 6, 25, "Restauracje"),
        new("Żabka", 18.99m, TransactionCurrency.PLN, TransactionType.Expense, 6, 25, "Jedzenie"),

        new("Empik", 74.80m, TransactionCurrency.PLN, TransactionType.Expense, 6, 26, "Edukacja", "Książki"),

        new("Carrefour", 91.25m, TransactionCurrency.PLN, TransactionType.Expense, 6, 27, "Jedzenie"),

        new("IKEA", 142.50m, TransactionCurrency.PLN, TransactionType.Expense, 6, 28, "Zakupy"),

        new("Ryanair", 89.99m, TransactionCurrency.EUR, TransactionType.Expense, 6, 29, "Podróże", "Lot"),

        new("Lidl", 110.20m, TransactionCurrency.PLN, TransactionType.Expense, 6, 30, "Jedzenie"),
        new("H&M", 159.99m, TransactionCurrency.PLN, TransactionType.Expense, 6, 30, "Zakupy"),

        // Kwiecień
        new("Netflix", 43.00m, TransactionCurrency.PLN, TransactionType.Expense, 4, 1, "Subskrypcje"),
        new("Spotify", 23.99m, TransactionCurrency.PLN, TransactionType.Expense, 4, 1, "Subskrypcje"),
        new("iCloud+", 14.99m, TransactionCurrency.PLN, TransactionType.Expense, 4, 1, "Subskrypcje"),

        new("Biedronka", 94.20m, TransactionCurrency.PLN, TransactionType.Expense, 4, 2, "Jedzenie"),

        new("McDonald's", 28.40m, TransactionCurrency.PLN, TransactionType.Expense, 4, 3, "Restauracje"),
        new("Orlen", 262.30m, TransactionCurrency.PLN, TransactionType.Expense, 4, 3, "Transport", "Tankowanie"),

        new("Zalando", 174.50m, TransactionCurrency.PLN, TransactionType.Expense, 4, 5, "Zakupy"),
        new("Żabka", 15.30m, TransactionCurrency.PLN, TransactionType.Expense, 4, 5, "Jedzenie"),

        new("Lidl", 112.80m, TransactionCurrency.PLN, TransactionType.Expense, 4, 6, "Jedzenie"),

        new("Apteka DOZ", 41.60m, TransactionCurrency.PLN, TransactionType.Expense, 4, 7, "Zdrowie"),

        new("Bolt", 27.90m, TransactionCurrency.PLN, TransactionType.Expense, 4, 8, "Transport"),

        new("Czynsz", 1800.00m, TransactionCurrency.PLN, TransactionType.Expense, 4, 10, "Mieszkanie i rachunki"),
        new("Pracodawca Sp. z o.o.", 8500.00m, TransactionCurrency.PLN, TransactionType.Income, 4, 10, "Inne", "Wynagrodzenie"),

        new("Pasibus", 51.50m, TransactionCurrency.PLN, TransactionType.Expense, 4, 11, "Restauracje"),

        new("MPK Wrocław", 120.00m, TransactionCurrency.PLN, TransactionType.Expense, 4, 12, "Transport", "Bilet miesięczny"),
        new("Booking.com", 145.00m, TransactionCurrency.EUR, TransactionType.Expense, 4, 12, "Podróże", "Nocleg"),

        new("Cinema City", 52.00m, TransactionCurrency.PLN, TransactionType.Expense, 4, 13, "Rozrywka"),

        new("Auchan", 188.90m, TransactionCurrency.PLN, TransactionType.Expense, 4, 14, "Jedzenie"),

        new("Tauron", 132.70m, TransactionCurrency.PLN, TransactionType.Expense, 4, 15, "Mieszkanie i rachunki", "Prąd"),
        new("PGNiG", 121.40m, TransactionCurrency.PLN, TransactionType.Expense, 4, 15, "Mieszkanie i rachunki", "Gaz"),
        new("Vectra", 79.00m, TransactionCurrency.PLN, TransactionType.Expense, 4, 15, "Mieszkanie i rachunki", "Internet"),

        new("Media Expert", 219.00m, TransactionCurrency.PLN, TransactionType.Expense, 4, 17, "Zakupy"),

        new("KFC", 35.60m, TransactionCurrency.PLN, TransactionType.Expense, 4, 18, "Restauracje"),
        new("Freelance - projekt", 1200.00m, TransactionCurrency.PLN, TransactionType.Income, 4, 18, "Inne"),

        new("Medicover", 220.00m, TransactionCurrency.PLN, TransactionType.Expense, 4, 19, "Zdrowie", "Wizyta prywatna"),

        new("Biedronka", 72.30m, TransactionCurrency.PLN, TransactionType.Expense, 4, 20, "Jedzenie"),

        new("Steam", 64.99m, TransactionCurrency.PLN, TransactionType.Expense, 4, 21, "Rozrywka", "Gra"),

        new("BP", 241.80m, TransactionCurrency.PLN, TransactionType.Expense, 4, 22, "Transport", "Tankowanie"),

        new("Udemy", 44.99m, TransactionCurrency.PLN, TransactionType.Expense, 4, 23, "Edukacja", "Kurs online"),

        new("Bankomat", 150.00m, TransactionCurrency.PLN, TransactionType.Expense, 4, 24, "Inne", "Wypłata gotówki"),

        new("Sphinx", 96.50m, TransactionCurrency.PLN, TransactionType.Expense, 4, 25, "Restauracje"),
        new("Żabka", 21.40m, TransactionCurrency.PLN, TransactionType.Expense, 4, 25, "Jedzenie"),

        new("Empik", 58.90m, TransactionCurrency.PLN, TransactionType.Expense, 4, 26, "Edukacja", "Książki"),

        new("Carrefour", 103.60m, TransactionCurrency.PLN, TransactionType.Expense, 4, 27, "Jedzenie"),

        new("IKEA", 89.90m, TransactionCurrency.PLN, TransactionType.Expense, 4, 28, "Zakupy"),

        new("Ryanair", 119.99m, TransactionCurrency.EUR, TransactionType.Expense, 4, 29, "Podróże", "Lot"),

        new("Lidl", 128.40m, TransactionCurrency.PLN, TransactionType.Expense, 4, 30, "Jedzenie"),
        new("H&M", 134.50m, TransactionCurrency.PLN, TransactionType.Expense, 4, 30, "Zakupy"),

        // Maj
        new("Netflix", 43.00m, TransactionCurrency.PLN, TransactionType.Expense, 5, 1, "Subskrypcje"),
        new("Spotify", 23.99m, TransactionCurrency.PLN, TransactionType.Expense, 5, 1, "Subskrypcje"),
        new("iCloud+", 14.99m, TransactionCurrency.PLN, TransactionType.Expense, 5, 1, "Subskrypcje"),

        new("Biedronka", 101.70m, TransactionCurrency.PLN, TransactionType.Expense, 5, 2, "Jedzenie"),

        new("McDonald's", 37.80m, TransactionCurrency.PLN, TransactionType.Expense, 5, 3, "Restauracje"),
        new("Orlen", 248.90m, TransactionCurrency.PLN, TransactionType.Expense, 5, 3, "Transport", "Tankowanie"),

        new("Zalando", 199.00m, TransactionCurrency.PLN, TransactionType.Expense, 5, 5, "Zakupy"),
        new("Żabka", 9.90m, TransactionCurrency.PLN, TransactionType.Expense, 5, 5, "Jedzenie"),

        new("Lidl", 96.50m, TransactionCurrency.PLN, TransactionType.Expense, 5, 6, "Jedzenie"),

        new("Apteka DOZ", 63.20m, TransactionCurrency.PLN, TransactionType.Expense, 5, 7, "Zdrowie"),

        new("Bolt", 19.80m, TransactionCurrency.PLN, TransactionType.Expense, 5, 8, "Transport"),

        new("Czynsz", 1800.00m, TransactionCurrency.PLN, TransactionType.Expense, 5, 10, "Mieszkanie i rachunki"),
        new("Pracodawca Sp. z o.o.", 8500.00m, TransactionCurrency.PLN, TransactionType.Income, 5, 10, "Inne", "Wynagrodzenie"),

        new("Pasibus", 42.00m, TransactionCurrency.PLN, TransactionType.Expense, 5, 11, "Restauracje"),

        new("MPK Wrocław", 120.00m, TransactionCurrency.PLN, TransactionType.Expense, 5, 12, "Transport", "Bilet miesięczny"),
        new("Booking.com", 98.50m, TransactionCurrency.EUR, TransactionType.Expense, 5, 12, "Podróże", "Nocleg"),

        new("Cinema City", 39.00m, TransactionCurrency.PLN, TransactionType.Expense, 5, 13, "Rozrywka"),

        new("Auchan", 217.40m, TransactionCurrency.PLN, TransactionType.Expense, 5, 14, "Jedzenie"),

        new("Tauron", 151.90m, TransactionCurrency.PLN, TransactionType.Expense, 5, 15, "Mieszkanie i rachunki", "Prąd"),
        new("PGNiG", 88.30m, TransactionCurrency.PLN, TransactionType.Expense, 5, 15, "Mieszkanie i rachunki", "Gaz"),
        new("Vectra", 79.00m, TransactionCurrency.PLN, TransactionType.Expense, 5, 15, "Mieszkanie i rachunki", "Internet"),

        new("Media Expert", 449.00m, TransactionCurrency.PLN, TransactionType.Expense, 5, 17, "Zakupy"),

        new("KFC", 44.20m, TransactionCurrency.PLN, TransactionType.Expense, 5, 18, "Restauracje"),
        new("Freelance - projekt", 1800.00m, TransactionCurrency.PLN, TransactionType.Income, 5, 18, "Inne"),

        new("Medicover", 140.00m, TransactionCurrency.PLN, TransactionType.Expense, 5, 19, "Zdrowie", "Wizyta prywatna"),

        new("Biedronka", 59.40m, TransactionCurrency.PLN, TransactionType.Expense, 5, 20, "Jedzenie"),

        new("Steam", 129.99m, TransactionCurrency.PLN, TransactionType.Expense, 5, 21, "Rozrywka", "Gra"),

        new("BP", 274.60m, TransactionCurrency.PLN, TransactionType.Expense, 5, 22, "Transport", "Tankowanie"),

        new("Udemy", 74.99m, TransactionCurrency.PLN, TransactionType.Expense, 5, 23, "Edukacja", "Kurs online"),

        new("Bankomat", 250.00m, TransactionCurrency.PLN, TransactionType.Expense, 5, 24, "Inne", "Wypłata gotówki"),

        new("Sphinx", 78.00m, TransactionCurrency.PLN, TransactionType.Expense, 5, 25, "Restauracje"),
        new("Żabka", 16.50m, TransactionCurrency.PLN, TransactionType.Expense, 5, 25, "Jedzenie"),

        new("Empik", 82.10m, TransactionCurrency.PLN, TransactionType.Expense, 5, 26, "Edukacja", "Książki"),

        new("Carrefour", 84.70m, TransactionCurrency.PLN, TransactionType.Expense, 5, 27, "Jedzenie"),

        new("IKEA", 169.90m, TransactionCurrency.PLN, TransactionType.Expense, 5, 28, "Zakupy"),

        new("Ryanair", 64.99m, TransactionCurrency.EUR, TransactionType.Expense, 5, 29, "Podróże", "Lot"),

        new("Lidl", 119.30m, TransactionCurrency.PLN, TransactionType.Expense, 5, 30, "Jedzenie"),
        new("H&M", 179.99m, TransactionCurrency.PLN, TransactionType.Expense, 5, 30, "Zakupy"),
    ];
}
