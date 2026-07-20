namespace api.Models;

public class Expense
{
    public required int Id { get; set; }
    public required DateOnly Date { get; set; }
    public required string Month { get; set; }
    public required string Day { get; set; }
    public required string Name { get; set; }
    public required string Group { get; set; }
    public required string Category { get; set; }
    public required string Tag { get; set; }
    public required decimal Amount { get; set; }
    public string? Description { get; set; } = string.Empty;

    public List<object> ToSpreadsheetRow()
    {
        return
        [
            Id,
            Date.ToString("yyyy-MM-dd"),
            Month,
            Day,
            Name,
            Group,
            Category,
            Tag,
            (double) Amount,
            Description ?? string.Empty
        ];
    }
}