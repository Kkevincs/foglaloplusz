using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Salonic.Migrations
{
    /// <inheritdoc />
    public partial class addedmorecoulumnstoappointment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "emailAddress",
                table: "Appointments",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "phoneNumber",
                table: "Appointments",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "emailAddress",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "phoneNumber",
                table: "Appointments");
        }
    }
}
