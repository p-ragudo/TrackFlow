namespace api.Models;

public class PaymentTemplate
{
    public required int Id { get; set; }
    public required string Name { get; set; }
    public required decimal Amount { get; set; }
    public string? Description { get; set; } = string.Empty;

    public List<object> ToSpreadsheetRow()
    {
        return
        [
            Id,
            Name,
            Amount,
            Description ?? string.Empty
        ];
    }
}