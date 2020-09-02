using Microsoft.EntityFrameworkCore.Migrations;

namespace AspNetIdentityDemo.Api.Migrations
{
    public partial class add4 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_AspNetUsers_UsersId",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Employees_UsersId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "UsersId",
                table: "Employees");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_AspNetUsers_UserID",
                table: "Employees",
                column: "UserID",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_AspNetUsers_UserID",
                table: "Employees");

            migrationBuilder.AddColumn<string>(
                name: "UsersId",
                table: "Employees",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_UsersId",
                table: "Employees",
                column: "UsersId");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_AspNetUsers_UsersId",
                table: "Employees",
                column: "UsersId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
