using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using PersonalFinanceTracker.Application.Abstractions;
using PersonalFinanceTracker.Application.Services;
using PersonalFinanceTracker.Infrastructure.Identity;
using PersonalFinanceTracker.Infrastructure.Imports;
using PersonalFinanceTracker.Infrastructure.Persistence;

namespace PersonalFinanceTracker.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(connectionString));
        services.AddScoped<IExpenseRepository, ExpenseRepository>();
        services.AddScoped<IExpenseCategoryRepository, ExpenseCategoryRepository>();
        services.AddScoped<IExpenseService, ExpenseService>();
        services.AddScoped<IExpenseStatementImportService, ExpenseStatementImportService>();
        services.AddScoped<IExpenseStatementParser, GoPayStatementPdfParser>();
        services.AddScoped<IApplicationDatabaseInitializer, ApplicationDatabaseInitializer>();
        services.AddDatabaseDeveloperPageExceptionFilter();

        services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.SignIn.RequireConfirmedAccount = true;
                options.Stores.SchemaVersion = IdentitySchemaVersions.Version3;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

        services.AddSingleton<IdentityNoOpEmailSender>();
        services.AddSingleton<IEmailSender<ApplicationUser>>(provider =>
            provider.GetRequiredService<IdentityNoOpEmailSender>());
        services.AddSingleton<IEmailDeliveryStatus>(provider =>
            provider.GetRequiredService<IdentityNoOpEmailSender>());

        return services;
    }
}
