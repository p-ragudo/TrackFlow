namespace api.Dto;

using api.Models;

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

public record BootstrapResponse
(
    List<ExpenseTemplate> Templates,
    decimal TodayTotal,
    List<TodayExpenseItemResponse> TodayExpenses,
    Identifiers Identifiers,
    string name
);