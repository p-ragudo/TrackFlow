namespace backend.Dto;

public record PutTemplate
(
    string SpreadsheetId,
    string RowRange,
    string Name,
    string Category,
    string Tag,
    decimal Amount,
    string Description
);