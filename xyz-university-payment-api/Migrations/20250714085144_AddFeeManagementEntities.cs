using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace xyz_university_payment_api.Migrations
{
    /// <inheritdoc />
    public partial class AddFeeManagementEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AdditionalFees",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Frequency = table.Column<int>(type: "int", nullable: false),
                    ApplicableTo = table.Column<int>(type: "int", nullable: false),
                    ApplicablePrograms = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ApplicableClasses = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ApplicableStudents = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdditionalFees", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FeeCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Frequency = table.Column<int>(type: "int", nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeeCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FeeStructures",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    AcademicYear = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Semester = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeeStructures", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StudentAdditionalFees",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AdditionalFeeId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentAdditionalFees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentAdditionalFees_AdditionalFees_AdditionalFeeId",
                        column: x => x.AdditionalFeeId,
                        principalTable: "AdditionalFees",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StudentAdditionalFees_Students_StudentNumber",
                        column: x => x.StudentNumber,
                        principalTable: "Students",
                        principalColumn: "StudentNumber");
                });

            migrationBuilder.CreateTable(
                name: "FeeStructureItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FeeStructureId = table.Column<int>(type: "int", nullable: false),
                    FeeCategoryId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeeStructureItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FeeStructureItems_FeeCategories_FeeCategoryId",
                        column: x => x.FeeCategoryId,
                        principalTable: "FeeCategories",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FeeStructureItems_FeeStructures_FeeStructureId",
                        column: x => x.FeeStructureId,
                        principalTable: "FeeStructures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StudentFeeAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FeeStructureId = table.Column<int>(type: "int", nullable: false),
                    AcademicYear = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Semester = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AssignedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentFeeAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentFeeAssignments_FeeStructures_FeeStructureId",
                        column: x => x.FeeStructureId,
                        principalTable: "FeeStructures",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StudentFeeAssignments_Students_StudentNumber",
                        column: x => x.StudentNumber,
                        principalTable: "Students",
                        principalColumn: "StudentNumber");
                });

            migrationBuilder.CreateTable(
                name: "StudentFeeBalances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FeeStructureItemId = table.Column<int>(type: "int", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AmountPaid = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    OutstandingBalance = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentFeeBalances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentFeeBalances_FeeStructureItems_FeeStructureItemId",
                        column: x => x.FeeStructureItemId,
                        principalTable: "FeeStructureItems",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StudentFeeBalances_Students_StudentNumber",
                        column: x => x.StudentNumber,
                        principalTable: "Students",
                        principalColumn: "StudentNumber");
                });

            migrationBuilder.CreateIndex(
                name: "IX_FeeCategories_Name",
                table: "FeeCategories",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FeeStructureItems_FeeCategoryId",
                table: "FeeStructureItems",
                column: "FeeCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_FeeStructureItems_FeeStructureId",
                table: "FeeStructureItems",
                column: "FeeStructureId");

            migrationBuilder.CreateIndex(
                name: "IX_FeeStructures_AcademicYear_Semester_Name",
                table: "FeeStructures",
                columns: new[] { "AcademicYear", "Semester", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentAdditionalFees_AdditionalFeeId",
                table: "StudentAdditionalFees",
                column: "AdditionalFeeId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentAdditionalFees_StudentNumber_AdditionalFeeId",
                table: "StudentAdditionalFees",
                columns: new[] { "StudentNumber", "AdditionalFeeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentFeeAssignments_FeeStructureId",
                table: "StudentFeeAssignments",
                column: "FeeStructureId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentFeeAssignments_StudentNumber_FeeStructureId_AcademicYear_Semester",
                table: "StudentFeeAssignments",
                columns: new[] { "StudentNumber", "FeeStructureId", "AcademicYear", "Semester" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentFeeBalances_FeeStructureItemId",
                table: "StudentFeeBalances",
                column: "FeeStructureItemId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentFeeBalances_StudentNumber_FeeStructureItemId",
                table: "StudentFeeBalances",
                columns: new[] { "StudentNumber", "FeeStructureItemId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StudentAdditionalFees");

            migrationBuilder.DropTable(
                name: "StudentFeeAssignments");

            migrationBuilder.DropTable(
                name: "StudentFeeBalances");

            migrationBuilder.DropTable(
                name: "AdditionalFees");

            migrationBuilder.DropTable(
                name: "FeeStructureItems");

            migrationBuilder.DropTable(
                name: "FeeCategories");

            migrationBuilder.DropTable(
                name: "FeeStructures");
        }
    }
}
