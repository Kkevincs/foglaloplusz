using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foglaloplusz.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddSalonLocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "Salons",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Location",
                table: "Salons");
        }
    }
}
