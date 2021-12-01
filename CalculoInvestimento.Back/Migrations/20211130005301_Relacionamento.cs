using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CalculoInvestimento.Back.Migrations
{
    public partial class Relacionamento : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Investiments",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Investiments_UserId",
                table: "Investiments",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Investiments_Users_UserId",
                table: "Investiments",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Investiments_Users_UserId",
                table: "Investiments");

            migrationBuilder.DropIndex(
                name: "IX_Investiments_UserId",
                table: "Investiments");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Investiments");
        }
    }
}
