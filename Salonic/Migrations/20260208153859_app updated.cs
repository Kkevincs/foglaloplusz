using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Salonic.Migrations
{
    /// <inheritdoc />
    public partial class appupdated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ServiceID",
                table: "Appointments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "wokerID",
                table: "Appointments",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ServiceID",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "wokerID",
                table: "Appointments");
        }
    }
}
