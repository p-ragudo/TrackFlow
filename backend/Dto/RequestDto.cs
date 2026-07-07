namespace backend.Dto;

public record CreateExpenseRequest
(
    string Category,
    string Tag,
    decimal Amount,
    string? Description
);

public record CreateTemplateRequest
(
    string Name,
    string Category,
    string Tag,
    decimal Amount,
    string? Description
);