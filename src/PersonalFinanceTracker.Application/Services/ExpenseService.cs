using PersonalFinanceTracker.Application.Abstractions;
using PersonalFinanceTracker.Application.Models;
using PersonalFinanceTracker.Domain.Entities;

namespace PersonalFinanceTracker.Application.Services;

public sealed class ExpenseService(
    IExpenseRepository expenses,
    IExpenseCategoryRepository categories) : IExpenseService
{
    public async Task<IReadOnlyList<ExpenseCategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        var items = await categories.ListAsync(cancellationToken);
        return items.Select(category => new ExpenseCategoryDto(category.Id, category.Name, category.ColorHex)).ToList();
    }

    public async Task<IReadOnlyList<ExpenseDto>> GetExpensesAsync(CancellationToken cancellationToken = default)
    {
        var items = await expenses.ListAsync(cancellationToken);
        return items.Select(ToDto).ToList();
    }

    public async Task<ExpenseDashboardDto> GetDashboardAsync(DateOnly month, Guid? categoryId = null, CancellationToken cancellationToken = default)
    {
        var currentMonth = new DateOnly(month.Year, month.Month, 1);
        var previousMonth = currentMonth.AddMonths(-1);
        var nextMonth = currentMonth.AddMonths(1);
        var items = await expenses.ListBetweenAsync(previousMonth, nextMonth, cancellationToken);
        var filtered = items.Where(expense => categoryId is null || expense.CategoryId == categoryId).ToList();
        var current = filtered.Where(expense => expense.SpentOn >= currentMonth && expense.SpentOn < nextMonth).ToList();
        var previousTotal = items
            .Where(expense => expense.SpentOn >= previousMonth && expense.SpentOn < currentMonth)
            .Where(expense => categoryId is null || expense.CategoryId == categoryId)
            .Sum(expense => expense.Amount);
        var total = current.Sum(expense => expense.Amount);

        var categorySpending = current
            .GroupBy(expense => expense.CategoryId)
            .Select(group => new CategorySpendingDto(
                group.Key,
                group.First().Category.Name,
                group.First().Category.ColorHex,
                group.Sum(expense => expense.Amount),
                group.Count(),
                total == 0 ? 0 : decimal.Round(group.Sum(expense => expense.Amount) * 100 / total, 1)))
            .OrderByDescending(category => category.Total)
            .ToList();

        var recent = current
            .OrderByDescending(expense => expense.SpentOn)
            .ThenByDescending(expense => expense.CreatedUtc)
            .Take(6)
            .Select(ToDto)
            .ToList();

        return new ExpenseDashboardDto(
            currentMonth,
            total,
            previousTotal,
            previousTotal == 0 ? null : decimal.Round((total - previousTotal) * 100 / previousTotal, 1),
            current.Count,
            categorySpending,
            recent,
            current.GroupBy(expense => expense.SpentOn.Day)
                .Select(group => new DailySpendingDto(group.Key, group.Sum(expense => expense.Amount)))
                .OrderBy(day => day.Day)
                .ToList());
    }

    public async Task<Guid> AddExpenseAsync(CreateExpenseRequest request, CancellationToken cancellationToken = default)
    {
        if (await categories.FindAsync(request.CategoryId, cancellationToken) is null)
            throw new InvalidOperationException("Choose a valid expense category.");

        var expense = Expense.Create(
            request.Description,
            request.Amount,
            request.SpentOn,
            request.CategoryId,
            request.Notes,
            importSource: request.ImportSource,
            externalReference: request.ExternalReference);
        expenses.Add(expense);
        await expenses.SaveChangesAsync(cancellationToken);
        return expense.Id;
    }

    public async Task<ExpenseImportResult> AddExpensesAsync(
        IReadOnlyList<CreateExpenseRequest> requests,
        CancellationToken cancellationToken = default)
    {
        if (requests.Count == 0)
            return new ExpenseImportResult(0, 0);

        var categoryIds = (await categories.ListAsync(cancellationToken)).Select(category => category.Id).ToHashSet();
        if (requests.Any(request => !categoryIds.Contains(request.CategoryId)))
            throw new InvalidOperationException("Choose a valid category for every imported expense.");

        var references = requests
            .Select(request => request.ExternalReference)
            .Where(reference => !string.IsNullOrWhiteSpace(reference))
            .Select(reference => reference!)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var existingReferences = await expenses.FindExistingExternalReferencesAsync(references, cancellationToken);
        var seenReferences = new HashSet<string>(StringComparer.Ordinal);
        var newExpenses = new List<Expense>(requests.Count);
        var duplicateCount = 0;

        foreach (var request in requests)
        {
            if (!string.IsNullOrWhiteSpace(request.ExternalReference)
                && (existingReferences.Contains(request.ExternalReference)
                    || !seenReferences.Add(request.ExternalReference)))
            {
                duplicateCount++;
                continue;
            }

            newExpenses.Add(Expense.Create(
                request.Description,
                request.Amount,
                request.SpentOn,
                request.CategoryId,
                request.Notes,
                importSource: request.ImportSource,
                externalReference: request.ExternalReference));
        }

        if (newExpenses.Count > 0)
        {
            expenses.AddRange(newExpenses);
            await expenses.SaveChangesAsync(cancellationToken);
        }

        return new ExpenseImportResult(newExpenses.Count, duplicateCount);
    }

    public async Task DeleteExpenseAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var expense = await expenses.FindAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException("The expense could not be found.");
        expenses.Remove(expense);
        await expenses.SaveChangesAsync(cancellationToken);
    }

    private static ExpenseDto ToDto(Expense expense) => new(
        expense.Id,
        expense.Description,
        expense.Amount,
        expense.SpentOn,
        expense.CategoryId,
        expense.Category.Name,
        expense.Category.ColorHex,
        expense.Notes,
        expense.IsSample,
        expense.ImportSource,
        expense.ExternalReference);
}
