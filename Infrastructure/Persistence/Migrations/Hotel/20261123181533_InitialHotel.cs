using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations.Hotel;

/// <inheritdoc />
public partial class _20261123181533_InitialHotel : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterDatabase()
            .Annotation("MySQL:Charset", "utf8mb4");

        migrationBuilder.CreateTable(
            name: "Guests",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "char(36)", nullable: false),
                FirstName = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
                LastName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                Email = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                Phone = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                Address = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false),
                IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                DeletedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Guests", x => x.Id);
            })
            .Annotation("MySQL:Charset", "utf8mb4");

        migrationBuilder.CreateTable(
            name: "RoomTypes",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "char(36)", nullable: false),
                Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                Capacity = table.Column<int>(type: "int", nullable: false),
                BaseRate = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_RoomTypes", x => x.Id);
            })
            .Annotation("MySQL:Charset", "utf8mb4");

        migrationBuilder.CreateTable(
            name: "Rooms",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "char(36)", nullable: false),
                Number = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                Floor = table.Column<int>(type: "int", nullable: false),
                IsOutOfService = table.Column<bool>(type: "tinyint(1)", nullable: false),
                RoomTypeId = table.Column<Guid>(type: "char(36)", nullable: false),
                CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                CreatedBy = table.Column<string>(type: "longtext", nullable: true),
                UpdatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                UpdatedBy = table.Column<string>(type: "longtext", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Rooms", x => x.Id);
                table.ForeignKey(
                    name: "FK_Rooms_RoomTypes_RoomTypeId",
                    column: x => x.RoomTypeId,
                    principalTable: "RoomTypes",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            })
            .Annotation("MySQL:Charset", "utf8mb4");

        migrationBuilder.CreateTable(
            name: "Bookings",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "char(36)", nullable: false),
                GuestId = table.Column<Guid>(type: "char(36)", nullable: false),
                RoomId = table.Column<Guid>(type: "char(36)", nullable: false),
                CheckInDate = table.Column<DateOnly>(type: "date", nullable: false),
                CheckOutDate = table.Column<DateOnly>(type: "date", nullable: false),
                TotalPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                Status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                IdempotencyKey = table.Column<Guid>(type: "char(36)", nullable: false),
                IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                DeletedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                CreatedBy = table.Column<string>(type: "longtext", nullable: true),
                UpdatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                UpdatedBy = table.Column<string>(type: "longtext", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Bookings", x => x.Id);
                table.ForeignKey(
                    name: "FK_Bookings_Guests_GuestId",
                    column: x => x.GuestId,
                    principalTable: "Guests",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_Bookings_Rooms_RoomId",
                    column: x => x.RoomId,
                    principalTable: "Rooms",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            })
            .Annotation("MySQL:Charset", "utf8mb4");

        migrationBuilder.CreateTable(
            name: "RoomImage",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "char(36)", nullable: false),
                RoomId = table.Column<Guid>(type: "char(36)", nullable: false),
                BlobName = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false),
                ContentType = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                SourceUrl = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                AltText = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: true),
                IsPrimary = table.Column<bool>(type: "tinyint(1)", nullable: false),
                SortOrder = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_RoomImage", x => x.Id);
                table.ForeignKey(
                    name: "FK_RoomImage_Rooms_RoomId",
                    column: x => x.RoomId,
                    principalTable: "Rooms",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            })
            .Annotation("MySQL:Charset", "utf8mb4");

        migrationBuilder.CreateIndex(
            name: "IX_Bookings_GuestId",
            table: "Bookings",
            column: "GuestId");

        migrationBuilder.CreateIndex(
            name: "IX_Bookings_IdempotencyKey",
            table: "Bookings",
            column: "IdempotencyKey",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Bookings_RoomId_CheckInDate_CheckOutDate",
            table: "Bookings",
            columns: new[] { "RoomId", "CheckInDate", "CheckOutDate" });

        migrationBuilder.CreateIndex(
            name: "IX_Guests_Email",
            table: "Guests",
            column: "Email",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_RoomImage_RoomId_SortOrder",
            table: "RoomImage",
            columns: new[] { "RoomId", "SortOrder" });

        migrationBuilder.CreateIndex(
            name: "IX_Rooms_RoomTypeId",
            table: "Rooms",
            column: "RoomTypeId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Bookings");

        migrationBuilder.DropTable(
            name: "RoomImage");

        migrationBuilder.DropTable(
            name: "Guests");

        migrationBuilder.DropTable(
            name: "Rooms");

        migrationBuilder.DropTable(
            name: "RoomTypes");
    }
}
