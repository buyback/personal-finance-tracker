namespace PersonalFinanceTracker.Domain.Entities;

public sealed class Expense
{
    private Expense() { }

    private Expense(
        Guid id,
        string description,
        decimal amount,
        DateOnly spentOn,
        Guid categoryId,
        string? notes,
        bool isSample,
        DateTimeOffset createdUtc,
        string? importSource,
        string? externalReference)
    {
        Id = id;
        Description = description;
        Amount = amount;
        SpentOn = spentOn;
        CategoryId = categoryId;
        Notes = notes;
        IsSample = isSample;
        CreatedUtc = createdUtc;
        ImportSource = importSource;
        ExternalReference = externalReference;
    }

    public Guid Id { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public decimal Amount { get; private set; }
    public DateOnly SpentOn { get; private set; }
    public Guid CategoryId { get; private set; }
    public string? Notes { get; private set; }
    public bool IsSample { get; private set; }
    public DateTimeOffset CreatedUtc { get; private set; }
    public string? ImportSource { get; private set; }
    public string? ExternalReference { get; private set; }
    public ExpenseCategory Category { get; private set; } = null!;

    public static Expense Create(
        string description,
        decimal amount,
        DateOnly spentOn,
        Guid categoryId,
        string? notes = null,
        bool isSample = false,
        string? importSource = null,
        string? externalReference = null)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("An expense description is required.", nameof(description));
        if (description.Trim().Length > 120)
            throw new ArgumentException("An expense description cannot exceed 120 characters.", nameof(description));
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "An expense amount must be greater than zero.");
        if (categoryId == Guid.Empty)
            throw new ArgumentException("An expense category is required.", nameof(categoryId));
        if (notes?.Length > 500)
            throw new ArgumentException("Expense notes cannot exceed 500 characters.", nameof(notes));
        if (importSource?.Length > 40)
            throw new ArgumentException("Import source cannot exceed 40 characters.", nameof(importSource));
        if (externalReference?.Length > 120)
            throw new ArgumentException("External reference cannot exceed 120 characters.", nameof(externalReference));
        if (!string.IsNullOrWhiteSpace(externalReference) && string.IsNullOrWhiteSpace(importSource))
            throw new ArgumentException("An import source is required when an external reference is provided.", nameof(importSource));

        return new Expense(
            Guid.NewGuid(),
            description.Trim(),
            decimal.Round(amount, 2, MidpointRounding.AwayFromZero),
            spentOn,
            categoryId,
            string.IsNullOrWhiteSpace(notes) ? null : notes.Trim(),
            isSample,
            DateTimeOffset.UtcNow,
            string.IsNullOrWhiteSpace(importSource) ? null : importSource.Trim(),
            string.IsNullOrWhiteSpace(externalReference) ? null : externalReference.Trim());
    }
}
