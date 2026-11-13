namespace PersonalFinanceTracker.Application.Abstractions;

public interface IApplicationDatabaseInitializer
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
}
