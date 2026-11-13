using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using PersonalFinanceTracker.Application.Abstractions;

namespace PersonalFinanceTracker.Infrastructure.Identity;

// Replace this development sender with a real provider before relying on email confirmation.
internal sealed class IdentityNoOpEmailSender : IEmailSender<ApplicationUser>, IEmailDeliveryStatus
{
    private readonly IEmailSender emailSender = new NoOpEmailSender();

    public bool IsConfigured => false;

    public Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink) =>
        emailSender.SendEmailAsync(email, "Confirm your email", $"Please confirm your account by <a href='{confirmationLink}'>clicking here</a>.");

    public Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink) =>
        emailSender.SendEmailAsync(email, "Reset your password", $"Please reset your password by <a href='{resetLink}'>clicking here</a>.");

    public Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode) =>
        emailSender.SendEmailAsync(email, "Reset your password", $"Please reset your password using the following code: {resetCode}");
}
