namespace Backend.Dto;

public record CreateExpenseRequest
(
    string Sheet,
    string Category,
    string Tag,
    decimal Amount,
    string? Description
);

public record CreateTemplateRequest
(
    string Sheet,
    string Name,
    string Category,
    string Tag,
    decimal Amount,
    string? Description
);