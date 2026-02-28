using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WalletWise.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CorrectedErrorRate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IDX_Transaction_Type",
                table: "Transactions");

            migrationBuilder.CreateIndex(
                name: "IDX_Transaction_Type",
                table: "Transactions",
                column: "Type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IDX_Transaction_Type",
                table: "Transactions");

            migrationBuilder.CreateIndex(
                name: "IDX_Transaction_Type",
                table: "Transactions",
                column: "Type",
                unique: true);
        }
    }
}
