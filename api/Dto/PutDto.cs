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