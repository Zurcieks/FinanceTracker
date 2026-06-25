namespace Api.Domain;

public class Transaction
{
    public Guid Id { get; set; }
    public string MerchantName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public decimal AmountInPLN { get; set; }
    public TransactionCurrency Currency { get; set; } = TransactionCurrency.PLN;
    public TransactionType Type { get; set; } = TransactionType.Expense;
    public string? ReceiptKey { get; set; }
    public DateOnly Date { get; set; }
    public DateTime CreatedAt { get; set; }

    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = null!;
}
