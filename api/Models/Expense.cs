namespace api.Models;

public class Expense
{
    private readonly DateOnly today = DateOnly.FromDateTime(DateTime.Now);

    public required int Id { get; set; }
    public string Date => today.ToString("yyyy-MM-dd");
    public string Month => today.ToString("MMMM");
    public string Day => today.ToString("dddd");
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
            Date,
            Month,
            Day,
            Name,
            Group,
            Category,
            Tag,
            (double)Amount,
            Description ?? string.Empty
        ];
    }
}