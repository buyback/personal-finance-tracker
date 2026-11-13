using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PersonalFinanceTracker.Infrastructure.Persistence.Migrations;

public partial class AddExpenseImportMetadata : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "ImportSource",
            table: "Expenses",
            type: "TEXT",
            maxLength: 40,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "ExternalReference",
            table: "Expenses",
            type: "TEXT",
            maxLength: 120,
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_Expenses_ExternalReference",
            table: "Expenses",
            column: "ExternalReference",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(name: "IX_Expenses_ExternalReference", table: "Expenses");
        migrationBuilder.DropColumn(name: "ExternalReference", table: "Expenses");
        migrationBuilder.DropColumn(name: "ImportSource", table: "Expenses");
    }
}
