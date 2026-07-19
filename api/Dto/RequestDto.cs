namespace api.Dto;

public record CreateExpenseRequest
(
    string Name,
    string Group,
    string Category,
    string Tag,
    decimal Amount,
    string? Description
);

public record CreateTemplateRequest
(
    string Name,
    string Group,
    string Category,
    string Tag,
    decimal Amount,
    string? Description
);