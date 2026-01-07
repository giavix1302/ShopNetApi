using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopNetApi.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueProductColor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductColors_Colors_ColorId",
                table: "ProductColors");

            migrationBuilder.DropIndex(
                name: "IX_ProductColors_ProductId",
                table: "ProductColors");

            migrationBuilder.CreateIndex(
                name: "IX_ProductColors_ProductId_ColorId",
                table: "ProductColors",
                columns: new[] { "ProductId", "ColorId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductColors_Colors_ColorId",
                table: "ProductColors",
                column: "ColorId",
                principalTable: "Colors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductColors_Colors_ColorId",
                table: "ProductColors");

            migrationBuilder.DropIndex(
                name: "IX_ProductColors_ProductId_ColorId",
                table: "ProductColors");

            migrationBuilder.CreateIndex(
                name: "IX_ProductColors_ProductId",
                table: "ProductColors",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductColors_Colors_ColorId",
                table: "ProductColors",
                column: "ColorId",
                principalTable: "Colors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
