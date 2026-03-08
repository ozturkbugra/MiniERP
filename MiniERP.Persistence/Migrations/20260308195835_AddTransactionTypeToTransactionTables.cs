using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiniERP.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTransactionTypeToTransactionTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "CustomerTransactions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "CashTransactions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "BankTransactions",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "CustomerTransactions");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "CashTransactions");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "BankTransactions");
        }
    }
}
