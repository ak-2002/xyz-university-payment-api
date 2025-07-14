using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace xyz_university_payment_api.Migrations
{
    /// <inheritdoc />
    public partial class AddBalanceTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "StudentNumber",
                table: "Students",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Program",
                table: "Students",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "Students",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                table: "Students",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Students",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "Students",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "PaymentPlanId",
                table: "PaymentNotifications",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StudentBalanceId",
                table: "PaymentNotifications",
                type: "int",
                nullable: true);

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Students_StudentNumber",
                table: "Students",
                column: "StudentNumber");

            migrationBuilder.CreateTable(
                name: "FeeSchedules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Semester = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AcademicYear = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Program = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TuitionFee = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    RegistrationFee = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    LibraryFee = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    LaboratoryFee = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    OtherFees = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeeSchedules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StudentBalances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FeeScheduleId = table.Column<int>(type: "int", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AmountPaid = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    OutstandingBalance = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentBalances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentBalances_FeeSchedules_FeeScheduleId",
                        column: x => x.FeeScheduleId,
                        principalTable: "FeeSchedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentBalances_Students_StudentNumber",
                        column: x => x.StudentNumber,
                        principalTable: "Students",
                        principalColumn: "StudentNumber",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PaymentPlans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    StudentBalanceId = table.Column<int>(type: "int", nullable: false),
                    PlanType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TotalInstallments = table.Column<int>(type: "int", nullable: false),
                    CompletedInstallments = table.Column<int>(type: "int", nullable: false),
                    InstallmentAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AmountPaid = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    RemainingAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentPlans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentPlans_StudentBalances_StudentBalanceId",
                        column: x => x.StudentBalanceId,
                        principalTable: "StudentBalances",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PaymentPlans_Students_StudentNumber",
                        column: x => x.StudentNumber,
                        principalTable: "Students",
                        principalColumn: "StudentNumber");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Students_StudentNumber",
                table: "Students",
                column: "StudentNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentNotifications_PaymentPlanId",
                table: "PaymentNotifications",
                column: "PaymentPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentNotifications_StudentBalanceId",
                table: "PaymentNotifications",
                column: "StudentBalanceId");

            migrationBuilder.CreateIndex(
                name: "IX_FeeSchedules_Semester_AcademicYear_Program",
                table: "FeeSchedules",
                columns: new[] { "Semester", "AcademicYear", "Program" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentPlans_StudentBalanceId",
                table: "PaymentPlans",
                column: "StudentBalanceId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentPlans_StudentNumber",
                table: "PaymentPlans",
                column: "StudentNumber");

            migrationBuilder.CreateIndex(
                name: "IX_StudentBalances_FeeScheduleId",
                table: "StudentBalances",
                column: "FeeScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentBalances_StudentNumber_FeeScheduleId",
                table: "StudentBalances",
                columns: new[] { "StudentNumber", "FeeScheduleId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentNotifications_PaymentPlans_PaymentPlanId",
                table: "PaymentNotifications",
                column: "PaymentPlanId",
                principalTable: "PaymentPlans",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentNotifications_StudentBalances_StudentBalanceId",
                table: "PaymentNotifications",
                column: "StudentBalanceId",
                principalTable: "StudentBalances",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PaymentNotifications_PaymentPlans_PaymentPlanId",
                table: "PaymentNotifications");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentNotifications_StudentBalances_StudentBalanceId",
                table: "PaymentNotifications");

            migrationBuilder.DropTable(
                name: "PaymentPlans");

            migrationBuilder.DropTable(
                name: "StudentBalances");

            migrationBuilder.DropTable(
                name: "FeeSchedules");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Students_StudentNumber",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_Students_StudentNumber",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_PaymentNotifications_PaymentPlanId",
                table: "PaymentNotifications");

            migrationBuilder.DropIndex(
                name: "IX_PaymentNotifications_StudentBalanceId",
                table: "PaymentNotifications");

            migrationBuilder.DropColumn(
                name: "PaymentPlanId",
                table: "PaymentNotifications");

            migrationBuilder.DropColumn(
                name: "StudentBalanceId",
                table: "PaymentNotifications");

            migrationBuilder.AlterColumn<string>(
                name: "StudentNumber",
                table: "Students",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "Program",
                table: "Students",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "Students",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                table: "Students",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Students",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "Students",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);
        }
    }
}
