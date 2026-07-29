namespace api.Dto;

public record PutTemplate
(
    string Name,
    string Group,
    string Category,
    string Tag,
    decimal Amount,
    string? Description
);

public record PutLoan
(
    int Id,
    string ColumnStart,
    string ColumnEnd,
    DateOnly Date,
    string Month,
    string Day,
    string Name,
    decimal Amount,
    string? Description
);