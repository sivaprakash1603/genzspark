using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusBooking.Api.Migrations
{
    /// <inheritdoc />
    public partial class FixOperatorOfficeForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OperatorOffices_OperatorProfiles_OperatorId",
                table: "OperatorOffices");

            migrationBuilder.AddForeignKey(
                name: "FK_OperatorOffices_Users_OperatorId",
                table: "OperatorOffices",
                column: "OperatorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OperatorOffices_Users_OperatorId",
                table: "OperatorOffices");

            migrationBuilder.AddForeignKey(
                name: "FK_OperatorOffices_OperatorProfiles_OperatorId",
                table: "OperatorOffices",
                column: "OperatorId",
                principalTable: "OperatorProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
