using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Salonic.DAL.Migrations
{
    /// <inheritdoc />
    public partial class ServiceSalonCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CreatedByArtistId",
                table: "Services",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SalonId",
                table: "Services",
                type: "int",
                nullable: true);

            migrationBuilder.Sql(@"
UPDATE `Services` s
SET `SalonId` = (
    SELECT a.`SalonId` FROM `ArtistService` j
    INNER JOIN `Artists` a ON a.`Id` = j.`ArtistsId`
    WHERE j.`ServicesId` = s.`Id`
    ORDER BY j.`ArtistsId` ASC LIMIT 1),
`CreatedByArtistId` = (
    SELECT MIN(j.`ArtistsId`) FROM `ArtistService` j WHERE j.`ServicesId` = s.`Id`)
WHERE EXISTS (SELECT 1 FROM `ArtistService` j WHERE j.`ServicesId` = s.`Id`);");

            migrationBuilder.Sql(@"
UPDATE `Services` s
SET `SalonId` = (SELECT `Id` FROM `Salons` ORDER BY `Id` LIMIT 1)
WHERE `SalonId` IS NULL;");

            migrationBuilder.AlterColumn<int>(
                name: "SalonId",
                table: "Services",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Services_CreatedByArtistId",
                table: "Services",
                column: "CreatedByArtistId");

            migrationBuilder.CreateIndex(
                name: "IX_Services_SalonId",
                table: "Services",
                column: "SalonId");

            migrationBuilder.AddForeignKey(
                name: "FK_Services_Artists_CreatedByArtistId",
                table: "Services",
                column: "CreatedByArtistId",
                principalTable: "Artists",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Services_Salons_SalonId",
                table: "Services",
                column: "SalonId",
                principalTable: "Salons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Services_Artists_CreatedByArtistId",
                table: "Services");

            migrationBuilder.DropForeignKey(
                name: "FK_Services_Salons_SalonId",
                table: "Services");

            migrationBuilder.DropIndex(
                name: "IX_Services_CreatedByArtistId",
                table: "Services");

            migrationBuilder.DropIndex(
                name: "IX_Services_SalonId",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "CreatedByArtistId",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "SalonId",
                table: "Services");
        }
    }
}
