using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Interchée.Migrations
{
    /// <inheritdoc />
    public partial class Addedsubmissionvar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "AssignmentId1",
                table: "AssignmentSubmissions",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AssignmentSubmissions_AssignmentId1",
                table: "AssignmentSubmissions",
                column: "AssignmentId1");

            migrationBuilder.AddForeignKey(
                name: "FK_AssignmentSubmissions_Assignments_AssignmentId1",
                table: "AssignmentSubmissions",
                column: "AssignmentId1",
                principalTable: "Assignments",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AssignmentSubmissions_Assignments_AssignmentId1",
                table: "AssignmentSubmissions");

            migrationBuilder.DropIndex(
                name: "IX_AssignmentSubmissions_AssignmentId1",
                table: "AssignmentSubmissions");

            migrationBuilder.DropColumn(
                name: "AssignmentId1",
                table: "AssignmentSubmissions");
        }
    }
}
