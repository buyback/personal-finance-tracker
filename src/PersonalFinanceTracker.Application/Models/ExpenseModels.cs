namespace PersonalFinanceTracker.Application.Models;

public sealed record ExpenseCategoryDto(Guid Id, string Name, string ColorHex);

public sealed record ExpenseDto(
    Guid Id,
    string Description,
    decimal Amount,
    DateOnly SpentOn,
    Guid CategoryId,
    string CategoryName,
    string CategoryColorHex,
    string? Notes,
    bool IsSample,
    string? ImportSource,
    string? ExternalReference);

public sealed record CreateExpenseRequest(
    string Description,
    decimal Amount,
    DateOnly SpentOn,
    Guid CategoryId,
    string? Notes = null,
    string? ImportSource = null,
    string? ExternalReference = null);

public sealed record ParsedStatementTransaction(
    string ExternalReference,
    string Description,
    decimal Amount,
    DateOnly SpentOn,
    string? PaymentMethod);

public sealed record ParsedExpenseStatement(
    string Provider,
    DateOnly PeriodStart,
    DateOnly PeriodEnd,
    decimal ExpenseTotal,
    int SkippedNonExpenseCount,
    IReadOnlyList<ParsedStatementTransaction> Expenses);

public sealed record ExpenseStatementPreviewItem(
    string ExternalReference,
    string Description,
    decimal Amount,
    DateOnly SpentOn,
    string? PaymentMethod,
    Guid CategoryId,
    bool IsDuplicate);

public sealed record ExpenseStatementPreview(
    string Provider,
    DateOnly PeriodStart,
    DateOnly PeriodEnd,
    decimal ExpenseTotal,
    int SkippedNonExpenseCount,
    IReadOnlyList<ExpenseCategoryDto> Categories,
    IReadOnlyList<ExpenseStatementPreviewItem> Items);

public sealed record ExpenseImportResult(int ImportedCount, int DuplicateCount);

public sealed record CategorySpendingDto(
    Guid CategoryId,
    string Name,
    string ColorHex,
    decimal Total,
    int ExpenseCount,
    decimal SharePercent);

public sealed record DailySpendingDto(int Day, decimal Total);

public sealed record ExpenseDashboardDto(
    DateOnly Month,
    decimal TotalSpent,
    decimal PreviousMonthTotal,
    decimal? MonthChangePercent,
    int ExpenseCount,
    IReadOnlyList<CategorySpendingDto> CategorySpending,
    IReadOnlyList<ExpenseDto> RecentExpenses,
    IReadOnlyList<DailySpendingDto> DailySpending);
