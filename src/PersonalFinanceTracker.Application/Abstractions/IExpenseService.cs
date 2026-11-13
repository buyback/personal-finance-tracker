using PersonalFinanceTracker.Application.Models;

namespace PersonalFinanceTracker.Application.Abstractions;

public interface IExpenseService
{
    Task<IReadOnlyList<ExpenseCategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ExpenseDto>> GetExpensesAsync(CancellationToken cancellationToken = default);
    Task<ExpenseDashboardDto> GetDashboardAsync(DateOnly month, Guid? categoryId = null, CancellationToken cancellationToken = default);
    Task<Guid> AddExpenseAsync(CreateExpenseRequest request, CancellationToken cancellationToken = default);
    Task<ExpenseImportResult> AddExpensesAsync(IReadOnlyList<CreateExpenseRequest> requests, CancellationToken cancellationToken = default);
    Task DeleteExpenseAsync(Guid id, CancellationToken cancellationToken = default);
}
