using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Interchée.Migrations
{
    /// <inheritdoc />
    public partial class AbsenceManagementModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Departments",
                newName: "DepartmentName");

            migrationBuilder.RenameIndex(
                name: "IX_Departments_Name",
                table: "Departments",
                newName: "IX_Departments_DepartmentName");

            migrationBuilder.AddColumn<int>(
                name: "DepartmentId",
                table: "Departments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Department",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DepartmentId",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DepartmentName",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DepartmentId",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "Department",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "DepartmentName",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "DepartmentName",
                table: "Departments",
                newName: "Name");

            migrationBuilder.RenameIndex(
                name: "IX_Departments_DepartmentName",
                table: "Departments",
                newName: "IX_Departments_Name");
        }
    }
}
