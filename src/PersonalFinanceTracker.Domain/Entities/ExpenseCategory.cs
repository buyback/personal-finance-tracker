namespace PersonalFinanceTracker.Domain.Entities;

public sealed class ExpenseCategory
{
    private ExpenseCategory() { }

    public ExpenseCategory(Guid id, string name, string colorHex)
    {
        if (id == Guid.Empty) throw new ArgumentException("A category ID is required.", nameof(id));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("A category name is required.", nameof(name));
        if (string.IsNullOrWhiteSpace(colorHex)) throw new ArgumentException("A category color is required.", nameof(colorHex));

        Id = id;
        Name = name.Trim();
        ColorHex = colorHex;
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string ColorHex { get; private set; } = "#64748B";
    public ICollection<Expense> Expenses { get; private set; } = new List<Expense>();
}
