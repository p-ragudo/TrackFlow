namespace backend.Dto;

public record CreateExpenseRequest
(
    string SpreadsheetId,
    string Sheet,
    string Category,
    string Tag,
    decimal Amount,
    string? Description
);

public record CreateTemplateRequest
(
    string SpreadsheetId,
    string Sheet,
    string Name,
    string Category,
    string Tag,
    decimal Amount,
    string? Description
);