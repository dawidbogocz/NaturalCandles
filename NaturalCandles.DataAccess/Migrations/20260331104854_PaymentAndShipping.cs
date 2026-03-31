 using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace NaturalCandles.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class PaymentAndShipping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmailAddress",
                table: "OrderHeaders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "ShippingMethodSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ShippingMethod = table.Column<int>(type: "int", nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    RequiresPickupPoint = table.Column<bool>(type: "bit", nullable: false),
                    SupportsCashOnDelivery = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShippingMethodSettings", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "ShippingMethodSettings",
                columns: new[] { "Id", "DisplayName", "IsEnabled", "Price", "RequiresPickupPoint", "ShippingMethod", "SortOrder", "SupportsCashOnDelivery" },
                values: new object[,]
                {
                    { 1, "InPost Paczkomat", true, 14.99m, true, 1, 1, false },
                    { 2, "InPost Courier", true, 16.99m, false, 2, 2, true },
                    { 3, "DPD Courier", true, 18.99m, false, 3, 3, true },
                    { 4, "ORLEN Paczka", true, 12.99m, true, 4, 4, false },
                    { 5, "Local Pickup", true, 0m, false, 5, 5, false }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShippingMethodSettings");

            migrationBuilder.DropColumn(
                name: "EmailAddress",
                table: "OrderHeaders");
        }
    }
}
