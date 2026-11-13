using Microsoft.EntityFrameworkCore;
using PersonalFinanceTracker.Application.Abstractions;
using PersonalFinanceTracker.Domain.Entities;

namespace PersonalFinanceTracker.Infrastructure.Persistence;

internal sealed class ExpenseRepository(ApplicationDbContext dbContext) : IExpenseRepository
{
    public async Task<IReadOnlyList<Expense>> ListAsync(CancellationToken cancellationToken = default) =>
        await dbContext.Expenses
            .AsNoTracking()
            .Include(expense => expense.Category)
            .OrderByDescending(expense => expense.SpentOn)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Expense>> ListBetweenAsync(
        DateOnly start,
        DateOnly endExclusive,
        CancellationToken cancellationToken = default) =>
        await dbContext.Expenses
            .AsNoTracking()
            .Include(expense => expense.Category)
            .Where(expense => expense.SpentOn >= start && expense.SpentOn < endExclusive)
            .OrderByDescending(expense => expense.SpentOn)
            .ToListAsync(cancellationToken);

    public Task<Expense?> FindAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Expenses.FirstOrDefaultAsync(expense => expense.Id == id, cancellationToken);

    public async Task<IReadOnlySet<string>> FindExistingExternalReferencesAsync(
        IReadOnlyCollection<string> references,
        CancellationToken cancellationToken = default)
    {
        if (references.Count == 0)
            return new HashSet<string>(StringComparer.Ordinal);

        return (await dbContext.Expenses.AsNoTracking()
                .Where(expense => expense.ExternalReference != null && references.Contains(expense.ExternalReference))
                .Select(expense => expense.ExternalReference!)
                .ToListAsync(cancellationToken))
            .ToHashSet(StringComparer.Ordinal);
    }

    public void Add(Expense expense) => dbContext.Expenses.Add(expense);

    public void AddRange(IReadOnlyCollection<Expense> expenses) => dbContext.Expenses.AddRange(expenses);

    public void Remove(Expense expense) => dbContext.Expenses.Remove(expense);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
