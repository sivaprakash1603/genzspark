using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusBooking.Api.Migrations
{
    /// <inheritdoc />
    public partial class FleetManagementArchitecture : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Seats_Buses_BusId",
                table: "Seats");

            migrationBuilder.DropColumn(
                name: "VehicleNumber",
                table: "OperatorProfiles");

            migrationBuilder.DropColumn(
                name: "BusName",
                table: "Buses");

            migrationBuilder.DropColumn(
                name: "SeatLayoutType",
                table: "Buses");

            migrationBuilder.DropColumn(
                name: "TotalSeats",
                table: "Buses");

            migrationBuilder.RenameColumn(
                name: "BusId",
                table: "Seats",
                newName: "VehicleId");

            migrationBuilder.RenameIndex(
                name: "IX_Seats_BusId_SeatNumber",
                table: "Seats",
                newName: "IX_Seats_VehicleId_SeatNumber");

            migrationBuilder.AddColumn<Guid>(
                name: "VehicleId1",
                table: "Seats",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "VehicleId",
                table: "Buses",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "Vehicles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OperatorId = table.Column<Guid>(type: "uuid", nullable: false),
                    VehicleNumber = table.Column<string>(type: "text", nullable: false),
                    BusName = table.Column<string>(type: "text", nullable: false),
                    SeatLayoutType = table.Column<string>(type: "text", nullable: false),
                    TotalSeats = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vehicles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Vehicles_Users_OperatorId",
                        column: x => x.OperatorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Seats_VehicleId1",
                table: "Seats",
                column: "VehicleId1");

            migrationBuilder.CreateIndex(
                name: "IX_Buses_VehicleId",
                table: "Buses",
                column: "VehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_OperatorId",
                table: "Vehicles",
                column: "OperatorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Buses_Vehicles_VehicleId",
                table: "Buses",
                column: "VehicleId",
                principalTable: "Vehicles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Seats_Vehicles_VehicleId",
                table: "Seats",
                column: "VehicleId",
                principalTable: "Vehicles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Seats_Vehicles_VehicleId1",
                table: "Seats",
                column: "VehicleId1",
                principalTable: "Vehicles",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Buses_Vehicles_VehicleId",
                table: "Buses");

            migrationBuilder.DropForeignKey(
                name: "FK_Seats_Vehicles_VehicleId",
                table: "Seats");

            migrationBuilder.DropForeignKey(
                name: "FK_Seats_Vehicles_VehicleId1",
                table: "Seats");

            migrationBuilder.DropTable(
                name: "Vehicles");

            migrationBuilder.DropIndex(
                name: "IX_Seats_VehicleId1",
                table: "Seats");

            migrationBuilder.DropIndex(
                name: "IX_Buses_VehicleId",
                table: "Buses");

            migrationBuilder.DropColumn(
                name: "VehicleId1",
                table: "Seats");

            migrationBuilder.DropColumn(
                name: "VehicleId",
                table: "Buses");

            migrationBuilder.RenameColumn(
                name: "VehicleId",
                table: "Seats",
                newName: "BusId");

            migrationBuilder.RenameIndex(
                name: "IX_Seats_VehicleId_SeatNumber",
                table: "Seats",
                newName: "IX_Seats_BusId_SeatNumber");

            migrationBuilder.AddColumn<string>(
                name: "VehicleNumber",
                table: "OperatorProfiles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BusName",
                table: "Buses",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SeatLayoutType",
                table: "Buses",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "TotalSeats",
                table: "Buses",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_Seats_Buses_BusId",
                table: "Seats",
                column: "BusId",
                principalTable: "Buses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
