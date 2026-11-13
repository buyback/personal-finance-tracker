using PersonalFinanceTracker.Application.Models;

namespace PersonalFinanceTracker.Application.Abstractions;

public interface IExpenseStatementParser
{
    ParsedExpenseStatement Parse(Stream document, CancellationToken cancellationToken = default);
}

public interface IExpenseStatementImportService
{
    Task<ExpenseStatementPreview> PreviewAsync(Stream document, CancellationToken cancellationToken = default);
    Task<ExpenseImportResult> ImportAsync(ExpenseStatementPreview preview, IReadOnlyList<ExpenseStatementPreviewItem> items, CancellationToken cancellationToken = default);
}
