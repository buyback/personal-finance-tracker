using Microsoft.EntityFrameworkCore;
using PersonalFinanceTracker.Application.Abstractions;
using PersonalFinanceTracker.Domain.Entities;

namespace PersonalFinanceTracker.Infrastructure.Persistence;

internal sealed class ExpenseCategoryRepository(ApplicationDbContext dbContext) : IExpenseCategoryRepository
{
    public async Task<IReadOnlyList<ExpenseCategory>> ListAsync(CancellationToken cancellationToken = default) =>
        await dbContext.ExpenseCategories
            .AsNoTracking()
            .OrderBy(category => category.Name)
            .ToListAsync(cancellationToken);

    public Task<ExpenseCategory?> FindAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.ExpenseCategories.FirstOrDefaultAsync(category => category.Id == id, cancellationToken);
}
