using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace xyz_university_payment_api.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentMethodFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "PaymentNotifications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentMethod",
                table: "PaymentNotifications",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "M-Pesa");

            migrationBuilder.AddColumn<string>(
                name: "ReceiptNumber",
                table: "PaymentNotifications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TransactionId",
                table: "PaymentNotifications",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Notes",
                table: "PaymentNotifications");

            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "PaymentNotifications");

            migrationBuilder.DropColumn(
                name: "ReceiptNumber",
                table: "PaymentNotifications");

            migrationBuilder.DropColumn(
                name: "TransactionId",
                table: "PaymentNotifications");
        }
    }
}
