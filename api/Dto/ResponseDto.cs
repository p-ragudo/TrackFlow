namespace api.Dto;

public record TodayExpenseItemResponse
(
    int Id,
    string Month,
    string Day,
    string Name,
    string Group,
    string Category,
    string Tag,
    decimal Amount,
    string? Description
);