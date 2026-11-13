using System.Globalization;
using System.Text.RegularExpressions;
using PersonalFinanceTracker.Application.Abstractions;
using PersonalFinanceTracker.Application.Models;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace PersonalFinanceTracker.Infrastructure.Imports;

internal sealed class GoPayStatementPdfParser : IExpenseStatementParser
{
    private const int MaximumPageCount = 30;
    private const int MaximumTransactionCount = 1000;
    private const double LineTolerance = 2.0;

    private static readonly Regex TransactionDatePattern = new("^\\d{2}/\\d{2}/\\d{4}$", RegexOptions.Compiled);
    private static readonly Regex PeriodPattern = new(
        @"Periode\s+transaksi\s*:\s*(?<start>\d{1,2}\s+\p{L}+\s+\d{4})\s*-\s*(?<end>\d{1,2}\s+\p{L}+\s+\d{4})",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex AmountPattern = new("^(?<sign>[+-])?Rp(?<amount>[0-9.]+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public ParsedExpenseStatement Parse(Stream document, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);

        if (!document.CanRead)
            throw new InvalidDataException("The uploaded PDF could not be read.");

        using var pdf = PdfDocument.Open(document);
        if (pdf.NumberOfPages == 0 || pdf.NumberOfPages > MaximumPageCount)
            throw new InvalidDataException("This PDF does not look like a supported monthly statement.");

        var pages = pdf.GetPages().ToList();
        var documentText = string.Join('\n', pages.Select(page => page.Text));
        if (!documentText.Contains("gopay", StringComparison.OrdinalIgnoreCase)
            || !documentText.Contains("e-statement", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidDataException("This file is not a GoPay e-statement PDF. Choose a statement downloaded from GoPay.");
        }

        var periodMatch = PeriodPattern.Match(documentText);
        if (!periodMatch.Success)
            throw new InvalidDataException("The statement period could not be read. Use a GoPay monthly e-statement PDF.");

        var culture = CultureInfo.GetCultureInfo("id-ID");
        if (!DateTime.TryParseExact(periodMatch.Groups["start"].Value, ["d MMMM yyyy", "dd MMMM yyyy"], culture, DateTimeStyles.None, out var periodStart)
            || !DateTime.TryParseExact(periodMatch.Groups["end"].Value, ["d MMMM yyyy", "dd MMMM yyyy"], culture, DateTimeStyles.None, out var periodEnd))
        {
            throw new InvalidDataException("The dates in this statement could not be read.");
        }

        var parsedTransactions = new List<ParsedStatementTransaction>();
        var skippedNonExpenseCount = 0;
        var total = 0m;

        foreach (var page in pages)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var current = (CurrentTransaction?)null;

            foreach (var line in BuildLines(page.GetWords()))
            {
                cancellationToken.ThrowIfCancellationRequested();
                var dateWord = line.Words.FirstOrDefault(word =>
                    word.BoundingBox.Left < page.Width * 0.14
                    && TransactionDatePattern.IsMatch(word.Text));

                if (dateWord is not null)
                {
                    if (current is not null)
                        AddTransaction(current, parsedTransactions, ref total, ref skippedNonExpenseCount);

                    if (!DateOnly.TryParseExact(dateWord.Text, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var spentOn))
                        throw new InvalidDataException("A transaction date in this statement could not be read.");

                    current = new CurrentTransaction(spentOn);
                }

                if (current is null)
                    continue;

                foreach (var word in line.Words)
                {
                    var x = word.BoundingBox.Left;
                    if (x >= page.Width * 0.14 && x < page.Width * 0.44)
                        current.DescriptionParts.Add(word.Text);
                    else if (x >= page.Width * 0.44 && x < page.Width * 0.65)
                        current.ReferenceParts.Add(word.Text);
                    else if (x >= page.Width * 0.65 && x < page.Width * 0.87)
                        current.PaymentMethodParts.Add(word.Text);
                    else if (x >= page.Width * 0.87)
                        current.AmountText = word.Text;
                }
            }

            if (current is not null)
                AddTransaction(current, parsedTransactions, ref total, ref skippedNonExpenseCount);
        }

        if (parsedTransactions.Count == 0)
            throw new InvalidDataException("No expense rows were found. Use a text-based GoPay monthly e-statement PDF.");
        if (parsedTransactions.Count > MaximumTransactionCount)
            throw new InvalidDataException("This statement has too many transactions to import at once.");

        return new ParsedExpenseStatement(
            "GoPay",
            DateOnly.FromDateTime(periodStart),
            DateOnly.FromDateTime(periodEnd),
            total,
            skippedNonExpenseCount,
            parsedTransactions);
    }

    private static IReadOnlyList<TextLine> BuildLines(IEnumerable<Word> words)
    {
        var lines = new List<TextLine>();
        foreach (var word in words.OrderByDescending(item => item.BoundingBox.Top).ThenBy(item => item.BoundingBox.Left))
        {
            var line = lines.FirstOrDefault(item => Math.Abs(item.Top - word.BoundingBox.Top) <= LineTolerance);
            if (line is null)
            {
                line = new TextLine(word.BoundingBox.Top);
                lines.Add(line);
            }

            line.Words.Add(word);
        }

        return lines
            .OrderByDescending(line => line.Top)
            .Select(line => new TextLine(line.Top, line.Words.OrderBy(word => word.BoundingBox.Left).ToList()))
            .ToList();
    }

    private static void AddTransaction(
        CurrentTransaction transaction,
        ICollection<ParsedStatementTransaction> expenses,
        ref decimal total,
        ref int skippedNonExpenseCount)
    {
        if (string.IsNullOrWhiteSpace(transaction.AmountText))
            return;

        var amountMatch = AmountPattern.Match(transaction.AmountText);
        if (!amountMatch.Success
            || !decimal.TryParse(amountMatch.Groups["amount"].Value.Replace(".", string.Empty, StringComparison.Ordinal), NumberStyles.None, CultureInfo.InvariantCulture, out var unsignedAmount))
        {
            return;
        }

        var signedAmount = amountMatch.Groups["sign"].Value == "-" ? -unsignedAmount : unsignedAmount;
        if (signedAmount >= 0)
        {
            skippedNonExpenseCount++;
            return;
        }

        var description = NormalizeWords(transaction.DescriptionParts);
        var reference = NormalizeReference(transaction.ReferenceParts);
        if (string.IsNullOrWhiteSpace(description) || string.IsNullOrWhiteSpace(reference))
            return;

        var expenseAmount = decimal.Abs(signedAmount);
        expenses.Add(new ParsedStatementTransaction(
            $"gopay:{reference}",
            description,
            expenseAmount,
            transaction.SpentOn,
            NormalizeWords(transaction.PaymentMethodParts)));
        total += expenseAmount;
    }

    private static string NormalizeWords(IEnumerable<string> words) =>
        string.Join(' ', words.Select(word => word.Trim()).Where(word => word.Length > 0));

    private static string NormalizeReference(IEnumerable<string> words) =>
        string.Concat(words.Select(word => string.Concat(word.Where(character => !char.IsWhiteSpace(character)))));

    private sealed class CurrentTransaction(DateOnly spentOn)
    {
        public DateOnly SpentOn { get; } = spentOn;
        public List<string> DescriptionParts { get; } = [];
        public List<string> ReferenceParts { get; } = [];
        public List<string> PaymentMethodParts { get; } = [];
        public string? AmountText { get; set; }
    }

    private sealed class TextLine(double top, List<Word>? words = null)
    {
        public double Top { get; } = top;
        public List<Word> Words { get; } = words ?? [];
    }
}
