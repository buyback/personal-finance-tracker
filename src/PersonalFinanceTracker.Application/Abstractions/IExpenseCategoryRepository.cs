using PersonalFinanceTracker.Domain.Entities;

namespace PersonalFinanceTracker.Application.Abstractions;

public interface IExpenseCategoryRepository
{
    Task<IReadOnlyList<ExpenseCategory>> ListAsync(CancellationToken cancellationToken = default);
    Task<ExpenseCategory?> FindAsync(Guid id, CancellationToken cancellationToken = default);
}
