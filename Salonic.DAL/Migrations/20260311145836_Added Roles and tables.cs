using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Salonic.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddedRolesandtables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Salon_SalonId",
                table: "Appointments");

            migrationBuilder.DropForeignKey(
                name: "FK_Artists_Salon_SalonId",
                table: "Artists");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Salon",
                table: "Salon");

            migrationBuilder.RenameTable(
                name: "Salon",
                newName: "Salons");

            migrationBuilder.AddColumn<int>(
                name: "OwnerId",
                table: "Salons",
                type: "int",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Salons",
                table: "Salons",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Admins",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Email = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Admins", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Owners",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Role = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Owners", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Salons_OwnerId",
                table: "Salons",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Salons_SalonId",
                table: "Appointments",
                column: "SalonId",
                principalTable: "Salons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Artists_Salons_SalonId",
                table: "Artists",
                column: "SalonId",
                principalTable: "Salons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Salons_Owners_OwnerId",
                table: "Salons",
                column: "OwnerId",
                principalTable: "Owners",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Salons_SalonId",
                table: "Appointments");

            migrationBuilder.DropForeignKey(
                name: "FK_Artists_Salons_SalonId",
                table: "Artists");

            migrationBuilder.DropForeignKey(
                name: "FK_Salons_Owners_OwnerId",
                table: "Salons");

            migrationBuilder.DropTable(
                name: "Admins");

            migrationBuilder.DropTable(
                name: "Owners");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Salons",
                table: "Salons");

            migrationBuilder.DropIndex(
                name: "IX_Salons_OwnerId",
                table: "Salons");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Salons");

            migrationBuilder.RenameTable(
                name: "Salons",
                newName: "Salon");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Salon",
                table: "Salon",
                column: "Id");

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
    }
}
