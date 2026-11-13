using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using PersonalFinanceTracker.Infrastructure.Persistence;

#nullable disable

namespace PersonalFinanceTracker.Infrastructure.Persistence.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20261021120000_AddExpenseTracking")]
public partial class AddExpenseTracking : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "ExpenseCategories",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                Name = table.Column<string>(type: "TEXT", maxLength: 60, nullable: false),
                ColorHex = table.Column<string>(type: "TEXT", maxLength: 7, nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_ExpenseCategories", category => category.Id));

        migrationBuilder.CreateTable(
            name: "Expenses",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                Description = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                Amount = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                SpentOn = table.Column<DateOnly>(type: "TEXT", nullable: false),
                CategoryId = table.Column<Guid>(type: "TEXT", nullable: false),
                Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                IsSample = table.Column<bool>(type: "INTEGER", nullable: false),
                CreatedUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Expenses", expense => expense.Id);
                table.ForeignKey(
                    name: "FK_Expenses_ExpenseCategories_CategoryId",
                    column: expense => expense.CategoryId,
                    principalTable: "ExpenseCategories",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_ExpenseCategories_Name", table: "ExpenseCategories", column: "Name", unique: true);
        migrationBuilder.CreateIndex(
            name: "IX_Expenses_CategoryId", table: "Expenses", column: "CategoryId");
        migrationBuilder.CreateIndex(
            name: "IX_Expenses_SpentOn", table: "Expenses", column: "SpentOn");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "Expenses");
        migrationBuilder.DropTable(name: "ExpenseCategories");
    }
}
