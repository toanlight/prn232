using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class AddCreatedByUpdatedByToCustomer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "Customers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedBy",
                table: "Customers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customers_CreatedBy",
                table: "Customers",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_UpdatedBy",
                table: "Customers",
                column: "UpdatedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_Users_CreatedBy",
                table: "Customers",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_Users_UpdatedBy",
                table: "Customers",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Customers_Users_CreatedBy",
                table: "Customers");

            migrationBuilder.DropForeignKey(
                name: "FK_Customers_Users_UpdatedBy",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Customers_CreatedBy",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Customers_UpdatedBy",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Customers");
        }
    }
}
