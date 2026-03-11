using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Salonic.DAL.Migrations
{
    /// <inheritdoc />
    public partial class Salontableadded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SalonId",
                table: "Artists",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SalonId",
                table: "Appointments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Salon",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Salon", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Artists_SalonId",
                table: "Artists",
                column: "SalonId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_SalonId",
                table: "Appointments",
                column: "SalonId");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Salon_SalonId",
                table: "Appointments",
                column: "SalonId",
                principalTable: "Salon",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Artists_Salon_SalonId",
                table: "Artists",
                column: "SalonId",
                principalTable: "Salon",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Salon_SalonId",
                table: "Appointments");

            migrationBuilder.DropForeignKey(
                name: "FK_Artists_Salon_SalonId",
                table: "Artists");

            migrationBuilder.DropTable(
                name: "Salon");

            migrationBuilder.DropIndex(
                name: "IX_Artists_SalonId",
                table: "Artists");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_SalonId",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "SalonId",
                table: "Artists");

            migrationBuilder.DropColumn(
                name: "SalonId",
                table: "Appointments");
        }
    }
}
