using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PersonalFinanceTracker.Domain.Entities;
using PersonalFinanceTracker.Infrastructure.Identity;

namespace PersonalFinanceTracker.Infrastructure.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<ExpenseCategory> ExpenseCategories => Set<ExpenseCategory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ExpenseCategory>(entity =>
        {
            entity.ToTable("ExpenseCategories");
            entity.HasKey(category => category.Id);
            entity.Property(category => category.Name).HasMaxLength(60).IsRequired();
            entity.Property(category => category.ColorHex).HasMaxLength(7).IsRequired();
            entity.HasIndex(category => category.Name).IsUnique();
        });

        modelBuilder.Entity<Expense>(entity =>
        {
            entity.ToTable("Expenses");
            entity.HasKey(expense => expense.Id);
            entity.Property(expense => expense.Description).HasMaxLength(120).IsRequired();
            entity.Property(expense => expense.Amount).HasPrecision(18, 2).IsRequired();
            entity.Property(expense => expense.SpentOn).IsRequired();
            entity.Property(expense => expense.Notes).HasMaxLength(500);
            entity.Property(expense => expense.IsSample).IsRequired();
            entity.Property(expense => expense.CreatedUtc).IsRequired();
            entity.Property(expense => expense.ImportSource).HasMaxLength(40);
            entity.Property(expense => expense.ExternalReference).HasMaxLength(120);
            entity.HasIndex(expense => expense.SpentOn);
            entity.HasIndex(expense => expense.ExternalReference).IsUnique();
            entity.HasOne(expense => expense.Category)
                .WithMany(category => category.Expenses)
                .HasForeignKey(expense => expense.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
