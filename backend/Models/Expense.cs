namespace Backend.Models;

public class Expense
{
    public required int Id { get; set; }
    public required string Date { get; set; }
    public required string Month { get; set; }
    public required string Day { get; set; }
    public required string Category { get; set; }
    public required string Tag { get; set; }
    public required decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
}