using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace xyz_university_payment_api.Migrations
{
    /// <inheritdoc />
    public partial class FixRolesTableSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add missing columns to Roles table if they don't exist
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Roles' AND COLUMN_NAME = 'Name')
                BEGIN
                    ALTER TABLE Roles ADD Name NVARCHAR(50) NOT NULL DEFAULT 'DefaultRole';
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Roles' AND COLUMN_NAME = 'Description')
                BEGIN
                    ALTER TABLE Roles ADD Description NVARCHAR(200) NOT NULL DEFAULT 'Default Description';
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Roles' AND COLUMN_NAME = 'IsActive')
                BEGIN
                    ALTER TABLE Roles ADD IsActive BIT NOT NULL DEFAULT 1;
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Roles' AND COLUMN_NAME = 'CreatedAt')
                BEGIN
                    ALTER TABLE Roles ADD CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE();
                END
            ");

            // Update existing roles with proper names if they exist
            migrationBuilder.Sql(@"
                UPDATE Roles SET Name = 'Admin', Description = 'Full system administrator' WHERE Id = 1;
                UPDATE Roles SET Name = 'Manager', Description = 'Department manager with limited admin rights' WHERE Id = 2;
                UPDATE Roles SET Name = 'Staff', Description = 'Regular staff member' WHERE Id = 3;
                UPDATE Roles SET Name = 'Student', Description = 'Student user with limited access' WHERE Id = 4;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove the columns if needed to rollback
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Roles' AND COLUMN_NAME = 'CreatedAt')
                BEGIN
                    ALTER TABLE Roles DROP COLUMN CreatedAt;
                END
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Roles' AND COLUMN_NAME = 'IsActive')
                BEGIN
                    ALTER TABLE Roles DROP COLUMN IsActive;
                END
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Roles' AND COLUMN_NAME = 'Description')
                BEGIN
                    ALTER TABLE Roles DROP COLUMN Description;
                END
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Roles' AND COLUMN_NAME = 'Name')
                BEGIN
                    ALTER TABLE Roles DROP COLUMN Name;
                END
            ");
        }
    }
}
