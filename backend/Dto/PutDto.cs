namespace backend.Dto;

public record PutTemplate
(
    string Name,
    string Category,
    string Tag,
    decimal Amount,
    string Description
);