namespace backend.Models;

public class Template
{
    public required int Id { get; set; }
    public required string Name { get; set; }
    public required string Category { get; set; }
    public required string Tag { get; set; }
    public required decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
}