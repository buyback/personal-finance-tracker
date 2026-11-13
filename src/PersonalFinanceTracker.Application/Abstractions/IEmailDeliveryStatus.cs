namespace PersonalFinanceTracker.Application.Abstractions;

/// <summary>
/// Exposes whether outbound email is configured without coupling UI components to a provider.
/// </summary>
public interface IEmailDeliveryStatus
{
    bool IsConfigured { get; }
}
