using PersonalFinanceTracker.Domain.Entities;

namespace PersonalFinanceTracker.Application.Abstractions;

public interface IExpenseRepository
{
    Task<IReadOnlyList<Expense>> ListAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Expense>> ListBetweenAsync(DateOnly start, DateOnly endExclusive, CancellationToken cancellationToken = default);
    Task<Expense?> FindAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlySet<string>> FindExistingExternalReferencesAsync(IReadOnlyCollection<string> references, CancellationToken cancellationToken = default);
    void Add(Expense expense);
    void AddRange(IReadOnlyCollection<Expense> expenses);
    void Remove(Expense expense);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
