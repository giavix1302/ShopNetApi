using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopNetApi.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCartItemConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CartItems_Colors_ColorId",
                table: "CartItems");

            migrationBuilder.DropIndex(
                name: "IX_CartItems_CartId",
                table: "CartItems");

            migrationBuilder.AlterColumn<decimal>(
                name: "UnitPrice",
                table: "CartItems",
                type: "numeric",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_CartId_ProductId_ColorId",
                table: "CartItems",
                columns: new[] { "CartId", "ProductId", "ColorId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CartItems_Colors_ColorId",
                table: "CartItems",
                column: "ColorId",
                principalTable: "Colors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CartItems_Colors_ColorId",
                table: "CartItems");

            migrationBuilder.DropIndex(
                name: "IX_CartItems_CartId_ProductId_ColorId",
                table: "CartItems");

            migrationBuilder.AlterColumn<decimal>(
                name: "UnitPrice",
                table: "CartItems",
                type: "numeric",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_CartId",
                table: "CartItems",
                column: "CartId");

            migrationBuilder.AddForeignKey(
                name: "FK_CartItems_Colors_ColorId",
                table: "CartItems",
                column: "ColorId",
                principalTable: "Colors",
                principalColumn: "Id");
        }
    }
}
