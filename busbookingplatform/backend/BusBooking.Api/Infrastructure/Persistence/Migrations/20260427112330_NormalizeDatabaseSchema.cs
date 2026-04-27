using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusBooking.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class NormalizeDatabaseSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Buses_BusId",
                table: "Bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Users_PassengerId",
                table: "Bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_BookingSeats_Seats_SeatId",
                table: "BookingSeats");

            migrationBuilder.DropForeignKey(
                name: "FK_Buses_Routes_RouteId",
                table: "Buses");

            migrationBuilder.DropForeignKey(
                name: "FK_Buses_Users_OperatorId",
                table: "Buses");

            migrationBuilder.DropForeignKey(
                name: "FK_Buses_Vehicles_VehicleId",
                table: "Buses");

            migrationBuilder.DropForeignKey(
                name: "FK_Routes_Destinations_DestinationId",
                table: "Routes");

            migrationBuilder.DropForeignKey(
                name: "FK_Routes_Sources_SourceId",
                table: "Routes");

            migrationBuilder.DropForeignKey(
                name: "FK_Seats_Vehicles_VehicleId1",
                table: "Seats");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Roles_RoleId",
                table: "Users");

            // Create Locations table first
            migrationBuilder.CreateTable(
                name: "Locations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Locations", x => x.Id);
                });

            // Migrate data from Sources and Destinations
            // 1. Insert all from Sources
            migrationBuilder.Sql("INSERT INTO \"Locations\" (\"Id\", \"Name\", \"IsActive\") SELECT \"Id\", \"Name\", \"IsActive\" FROM \"Sources\";");
            // 2. Insert from Destinations where the city name doesn't already exist in Locations (to avoid UQ violation)
            migrationBuilder.Sql("INSERT INTO \"Locations\" (\"Id\", \"Name\", \"IsActive\") SELECT \"Id\", \"Name\", \"IsActive\" FROM \"Destinations\" WHERE \"Name\" NOT IN (SELECT \"Name\" FROM \"Locations\");");
            // 3. For any Routes that were pointing to a Destination ID that we didn't insert (because the name was duplicate), 
            //    update them to point to the Source ID with the same name.
            migrationBuilder.Sql("UPDATE \"Routes\" SET \"DestinationId\" = s.\"Id\" FROM \"Sources\" s, \"Destinations\" d WHERE \"Routes\".\"DestinationId\" = d.\"Id\" AND d.\"Name\" = s.\"Name\" AND d.\"Id\" NOT IN (SELECT \"Id\" FROM \"Locations\");");
            migrationBuilder.Sql("UPDATE \"Routes\" SET \"SourceId\" = s.\"Id\" FROM \"Sources\" s, \"Destinations\" d WHERE \"Routes\".\"SourceId\" = d.\"Id\" AND d.\"Name\" = s.\"Name\" AND d.\"Id\" NOT IN (SELECT \"Id\" FROM \"Locations\");");

            migrationBuilder.DropTable(
                name: "Destinations");

            migrationBuilder.DropTable(
                name: "Sources");

            migrationBuilder.DropIndex(
                name: "IX_Seats_VehicleId1",
                table: "Seats");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OperatorProfiles",
                table: "OperatorProfiles");

            migrationBuilder.DropIndex(
                name: "IX_OperatorProfiles_UserId",
                table: "OperatorProfiles");

            migrationBuilder.DropColumn(
                name: "VehicleId1",
                table: "Seats");

            migrationBuilder.RenameIndex(
                name: "IX_Users_Username",
                table: "Users",
                newName: "UQ_Users_Username");

            migrationBuilder.RenameIndex(
                name: "IX_Users_Email",
                table: "Users",
                newName: "UQ_Users_Email");

            migrationBuilder.RenameIndex(
                name: "IX_Tickets_TicketNumber",
                table: "Tickets",
                newName: "UQ_Tickets_Number");

            migrationBuilder.RenameIndex(
                name: "IX_Tickets_BookingId",
                table: "Tickets",
                newName: "UQ_Tickets_BookingId");

            migrationBuilder.RenameIndex(
                name: "IX_Seats_VehicleId_SeatNumber",
                table: "Seats",
                newName: "UQ_Seats_Vehicle_Number");

            migrationBuilder.RenameIndex(
                name: "IX_Routes_SourceId_DestinationId",
                table: "Routes",
                newName: "UQ_Routes_Source_Destination");

            migrationBuilder.RenameIndex(
                name: "IX_Roles_Name",
                table: "Roles",
                newName: "UQ_Roles_Name");

            migrationBuilder.RenameIndex(
                name: "IX_Refunds_BookingId",
                table: "Refunds",
                newName: "UQ_Refunds_BookingId");

            migrationBuilder.RenameIndex(
                name: "IX_Payments_TransactionId",
                table: "Payments",
                newName: "UQ_Payments_TransactionId");

            migrationBuilder.RenameIndex(
                name: "IX_OperatorOffices_OperatorId_CityName",
                table: "OperatorOffices",
                newName: "UQ_OperatorOffices_Operator_City");

            migrationBuilder.RenameIndex(
                name: "IX_BookingSeats_BookingId_SeatId",
                table: "BookingSeats",
                newName: "UQ_BookingSeats_Booking_Seat");

            migrationBuilder.RenameIndex(
                name: "IX_BookingPassengers_BookingId_SeatId",
                table: "BookingPassengers",
                newName: "UQ_BookingPassengers_Booking_Seat");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OperatorProfiles",
                table: "OperatorProfiles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "UQ_Vehicles_Number",
                table: "Vehicles",
                column: "VehicleNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Locations_Name",
                table: "Locations",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Buses_BusId",
                table: "Bookings",
                column: "BusId",
                principalTable: "Buses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Users_PassengerId",
                table: "Bookings",
                column: "PassengerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BookingSeats_Seats_SeatId",
                table: "BookingSeats",
                column: "SeatId",
                principalTable: "Seats",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Buses_Routes_RouteId",
                table: "Buses",
                column: "RouteId",
                principalTable: "Routes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Buses_Users_OperatorId",
                table: "Buses",
                column: "OperatorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Buses_Vehicles_VehicleId",
                table: "Buses",
                column: "VehicleId",
                principalTable: "Vehicles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Routes_Locations_DestinationId",
                table: "Routes",
                column: "DestinationId",
                principalTable: "Locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Routes_Locations_SourceId",
                table: "Routes",
                column: "SourceId",
                principalTable: "Locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Roles_RoleId",
                table: "Users",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Buses_BusId",
                table: "Bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Users_PassengerId",
                table: "Bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_BookingSeats_Seats_SeatId",
                table: "BookingSeats");

            migrationBuilder.DropForeignKey(
                name: "FK_Buses_Routes_RouteId",
                table: "Buses");

            migrationBuilder.DropForeignKey(
                name: "FK_Buses_Users_OperatorId",
                table: "Buses");

            migrationBuilder.DropForeignKey(
                name: "FK_Buses_Vehicles_VehicleId",
                table: "Buses");

            migrationBuilder.DropForeignKey(
                name: "FK_Routes_Locations_DestinationId",
                table: "Routes");

            migrationBuilder.DropForeignKey(
                name: "FK_Routes_Locations_SourceId",
                table: "Routes");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Roles_RoleId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "Locations");

            migrationBuilder.DropIndex(
                name: "UQ_Vehicles_Number",
                table: "Vehicles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OperatorProfiles",
                table: "OperatorProfiles");

            migrationBuilder.RenameIndex(
                name: "UQ_Users_Username",
                table: "Users",
                newName: "IX_Users_Username");

            migrationBuilder.RenameIndex(
                name: "UQ_Users_Email",
                table: "Users",
                newName: "IX_Users_Email");

            migrationBuilder.RenameIndex(
                name: "UQ_Tickets_Number",
                table: "Tickets",
                newName: "IX_Tickets_TicketNumber");

            migrationBuilder.RenameIndex(
                name: "UQ_Tickets_BookingId",
                table: "Tickets",
                newName: "IX_Tickets_BookingId");

            migrationBuilder.RenameIndex(
                name: "UQ_Seats_Vehicle_Number",
                table: "Seats",
                newName: "IX_Seats_VehicleId_SeatNumber");

            migrationBuilder.RenameIndex(
                name: "UQ_Routes_Source_Destination",
                table: "Routes",
                newName: "IX_Routes_SourceId_DestinationId");

            migrationBuilder.RenameIndex(
                name: "UQ_Roles_Name",
                table: "Roles",
                newName: "IX_Roles_Name");

            migrationBuilder.RenameIndex(
                name: "UQ_Refunds_BookingId",
                table: "Refunds",
                newName: "IX_Refunds_BookingId");

            migrationBuilder.RenameIndex(
                name: "UQ_Payments_TransactionId",
                table: "Payments",
                newName: "IX_Payments_TransactionId");

            migrationBuilder.RenameIndex(
                name: "UQ_OperatorOffices_Operator_City",
                table: "OperatorOffices",
                newName: "IX_OperatorOffices_OperatorId_CityName");

            migrationBuilder.RenameIndex(
                name: "UQ_BookingSeats_Booking_Seat",
                table: "BookingSeats",
                newName: "IX_BookingSeats_BookingId_SeatId");

            migrationBuilder.RenameIndex(
                name: "UQ_BookingPassengers_Booking_Seat",
                table: "BookingPassengers",
                newName: "IX_BookingPassengers_BookingId_SeatId");

            migrationBuilder.AddColumn<Guid>(
                name: "VehicleId1",
                table: "Seats",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_OperatorProfiles",
                table: "OperatorProfiles",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Destinations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Destinations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Sources",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sources", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Seats_VehicleId1",
                table: "Seats",
                column: "VehicleId1");

            migrationBuilder.CreateIndex(
                name: "IX_OperatorProfiles_UserId",
                table: "OperatorProfiles",
                column: "UserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Buses_BusId",
                table: "Bookings",
                column: "BusId",
                principalTable: "Buses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Users_PassengerId",
                table: "Bookings",
                column: "PassengerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BookingSeats_Seats_SeatId",
                table: "BookingSeats",
                column: "SeatId",
                principalTable: "Seats",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Buses_Routes_RouteId",
                table: "Buses",
                column: "RouteId",
                principalTable: "Routes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Buses_Users_OperatorId",
                table: "Buses",
                column: "OperatorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Buses_Vehicles_VehicleId",
                table: "Buses",
                column: "VehicleId",
                principalTable: "Vehicles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Routes_Destinations_DestinationId",
                table: "Routes",
                column: "DestinationId",
                principalTable: "Destinations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Routes_Sources_SourceId",
                table: "Routes",
                column: "SourceId",
                principalTable: "Sources",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Seats_Vehicles_VehicleId1",
                table: "Seats",
                column: "VehicleId1",
                principalTable: "Vehicles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Roles_RoleId",
                table: "Users",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
