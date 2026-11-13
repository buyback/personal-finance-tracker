using PersonalFinanceTracker.Application.Abstractions;
using PersonalFinanceTracker.Application.Models;

namespace PersonalFinanceTracker.Application.Services;

public sealed class ExpenseStatementImportService(
    IExpenseStatementParser parser,
    IExpenseService expenses,
    IExpenseCategoryRepository categories) : IExpenseStatementImportService
{
    private const int MaximumStatementBytes = 10 * 1024 * 1024;

    public async Task<ExpenseStatementPreview> PreviewAsync(
        Stream document,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);
        await using var bufferedDocument = new MemoryStream();
        var buffer = new byte[81920];
        int bytesRead;
        while ((bytesRead = await document.ReadAsync(buffer, cancellationToken)) > 0)
        {
            if (bufferedDocument.Length + bytesRead > MaximumStatementBytes)
                throw new InvalidDataException("The statement is larger than the 10 MB upload limit.");

            await bufferedDocument.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
        }

        bufferedDocument.Position = 0;
        var statement = parser.Parse(bufferedDocument, cancellationToken);
        var categoryEntities = await categories.ListAsync(cancellationToken);
        var categoryDtos = categoryEntities
            .Select(category => new ExpenseCategoryDto(category.Id, category.Name, category.ColorHex))
            .ToList();
        var categoryByName = categoryEntities.ToDictionary(category => category.Name, StringComparer.OrdinalIgnoreCase);
        var otherCategory = categoryEntities.FirstOrDefault(category => category.Name == "Other")
            ?? categoryEntities.FirstOrDefault()
            ?? throw new InvalidOperationException("No expense categories are available.");
        var existingReferences = (await expenses.GetExpensesAsync(cancellationToken))
            .Select(expense => expense.ExternalReference)
            .Where(reference => !string.IsNullOrWhiteSpace(reference))
            .ToHashSet(StringComparer.Ordinal);

        var items = statement.Expenses.Select(item => new ExpenseStatementPreviewItem(
            item.ExternalReference,
            item.Description,
            item.Amount,
            item.SpentOn,
            item.PaymentMethod,
            GuessCategory(item.Description, categoryByName, otherCategory.Id),
            existingReferences.Contains(item.ExternalReference))).ToList();

        return new ExpenseStatementPreview(
            statement.Provider,
            statement.PeriodStart,
            statement.PeriodEnd,
            statement.ExpenseTotal,
            statement.SkippedNonExpenseCount,
            categoryDtos,
            items);
    }

    public async Task<ExpenseImportResult> ImportAsync(
        ExpenseStatementPreview preview,
        IReadOnlyList<ExpenseStatementPreviewItem> items,
        CancellationToken cancellationToken = default)
    {
        var requests = items.Select(item => new CreateExpenseRequest(
            item.Description,
            item.Amount,
            item.SpentOn,
            item.CategoryId,
            item.PaymentMethod is null ? null : $"Payment method: {item.PaymentMethod}",
            preview.Provider,
            item.ExternalReference)).ToArray();

        return await expenses.AddExpensesAsync(requests, cancellationToken);
    }

    private static Guid GuessCategory(
        string description,
        IReadOnlyDictionary<string, PersonalFinanceTracker.Domain.Entities.ExpenseCategory> categories,
        Guid fallback)
    {
        var text = description.ToLowerInvariant();
        var suggested = text switch
        {
            _ when HasAny(text, "kost", "homestay", "hotel", "penginapan") => "Housing",
            _ when HasAny(text, "ferry", "goride", "transport", "bus", "toll", "taxi", "grab") => "Transport",
            _ when HasAny(text, "netflix", "spotify", "cinema", "bioskop", "movie") => "Entertainment",
            _ when HasAny(text, "hair", "salon", "barber", "beauty") => "Personal Care",
            _ when HasAny(text, "soup", "hangry", "goreng", "ayam", "kopi", "coffee", "dessert", "bebek", "nasi", "warung", "kedai", "sushi", "martabak", "donburo", "epok", "food") => "Food & Dining",
            _ => null
        };

        return suggested is not null && categories.TryGetValue(suggested, out var category)
            ? category.Id
            : fallback;
    }

    private static bool HasAny(string value, params string[] keywords) =>
        keywords.Any(value.Contains);
}
