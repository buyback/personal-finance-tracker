using Microsoft.EntityFrameworkCore;
using PersonalFinanceTracker.Application.Abstractions;
using PersonalFinanceTracker.Domain.Entities;

namespace PersonalFinanceTracker.Infrastructure.Persistence;

internal sealed class ApplicationDatabaseInitializer(ApplicationDbContext dbContext) : IApplicationDatabaseInitializer
{
    private static readonly (string Name, string Color)[] CategoryDefinitions =
    [
        ("Food & Dining", "#F28C45"),
        ("Transport", "#5B8DEF"),
        ("Groceries", "#36A982"),
        ("Bills & Utilities", "#8874D8"),
        ("Shopping", "#D86A9B"),
        ("Health", "#29A8A2"),
        ("Entertainment", "#D7A53E"),
        ("Housing", "#8B7765"),
        ("Personal Care", "#D86A9B"),
        ("Other", "#8290A2")
    ];

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.Database.MigrateAsync(cancellationToken);
        await dbContext.Expenses.Where(expense => expense.IsSample).ExecuteDeleteAsync(cancellationToken);

        var existingCategories = await dbContext.ExpenseCategories.ToListAsync(cancellationToken);
        var names = existingCategories.Select(category => category.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var definition in CategoryDefinitions.Where(definition => !names.Contains(definition.Name)))
        {
            dbContext.ExpenseCategories.Add(new ExpenseCategory(
                StableCategoryId(definition.Name), definition.Name, definition.Color));
        }
        await dbContext.SaveChangesAsync(cancellationToken);

    }

    private static Guid StableCategoryId(string name) => name switch
    {
        "Food & Dining" => Guid.Parse("a1000000-0000-0000-0000-000000000001"),
        "Transport" => Guid.Parse("a1000000-0000-0000-0000-000000000002"),
        "Groceries" => Guid.Parse("a1000000-0000-0000-0000-000000000003"),
        "Bills & Utilities" => Guid.Parse("a1000000-0000-0000-0000-000000000004"),
        "Shopping" => Guid.Parse("a1000000-0000-0000-0000-000000000005"),
        "Health" => Guid.Parse("a1000000-0000-0000-0000-000000000006"),
        "Entertainment" => Guid.Parse("a1000000-0000-0000-0000-000000000007"),
        "Other" => Guid.Parse("a1000000-0000-0000-0000-000000000008"),
        "Housing" => Guid.Parse("a1000000-0000-0000-0000-000000000009"),
        "Personal Care" => Guid.Parse("a1000000-0000-0000-0000-000000000010"),
        _ => Guid.NewGuid()
    };
}
