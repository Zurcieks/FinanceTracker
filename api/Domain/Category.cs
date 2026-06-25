namespace Api.Domain;

public class Category
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string HexColor { get; set; } = "#FFFFFF";
    public string Icon { get; set; } = "tag.fill";
    public bool IsDefault { get; set; } = false;
    public bool IsArchived { get; set; } = false;

    public List<Transaction> Transactions { get; set; } = [];
}
