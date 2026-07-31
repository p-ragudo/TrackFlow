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

public record BootstrapRequest
(
    string TemplateSheet,
    string ExpensesSheet,
    string TodayExpensesSheet,
    string IdentifiersSheet,
    string NameSheet
);

public record CreateLoanRequest
(
    string ColumnStart,
    string ColumnEnd,
    string CounterSheet,
    string RowCell,
    string IdCell,
    string Name,
    decimal Amount,
    string? Description
);

public record CreatePaymentRequest
(
    string ColumnStart,
    string ColumnEnd,
    string CounterSheet,
    string RowCell,
    string IdCell,
    string Name,
    decimal Amount,
    string? Description
);

public record CreatePaymentTemplateRequest
(
    string ColumnStart,
    string ColumnEnd,
    string CounterSheet,
    string RowCell,
    string IdCell,
    string Name,
    decimal Amount,
    string? Description
);