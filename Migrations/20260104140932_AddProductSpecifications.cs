using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopNetApi.Migrations
{
    /// <inheritdoc />
    public partial class AddProductSpecifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProductSpecifications_ProductId",
                table: "ProductSpecifications");

            migrationBuilder.AlterColumn<string>(
                name: "SpecValue",
                table: "ProductSpecifications",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "SpecName",
                table: "ProductSpecifications",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateIndex(
                name: "IX_ProductSpecifications_ProductId_SpecName",
                table: "ProductSpecifications",
                columns: new[] { "ProductId", "SpecName" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProductSpecifications_ProductId_SpecName",
                table: "ProductSpecifications");

            migrationBuilder.AlterColumn<string>(
                name: "SpecValue",
                table: "ProductSpecifications",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "SpecName",
                table: "ProductSpecifications",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.CreateIndex(
                name: "IX_ProductSpecifications_ProductId",
                table: "ProductSpecifications",
                column: "ProductId");
        }
    }
}
