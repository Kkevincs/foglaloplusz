using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Salonic.Migrations
{
    /// <inheritdoc />
    public partial class appointmentfixed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "Appointments");

            migrationBuilder.AddColumn<int>(
                name: "day",
                table: "Appointments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "hour",
                table: "Appointments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "minute",
                table: "Appointments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "month",
                table: "Appointments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "year",
                table: "Appointments",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "day",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "hour",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "minute",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "month",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "year",
                table: "Appointments");

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "Appointments",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
