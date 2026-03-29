using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Salonic.DAL.Migrations
{
    /// <inheritdoc />
    public partial class ServiceRequiresCreatedByArtist : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Services_Artists_CreatedByArtistId",
                table: "Services");

            migrationBuilder.Sql(@"
UPDATE `Services` s
SET `CreatedByArtistId` = (
    SELECT MIN(j.`ArtistsId`) FROM `ArtistService` j WHERE j.`ServicesId` = s.`Id`)
WHERE s.`CreatedByArtistId` IS NULL
  AND EXISTS (SELECT 1 FROM `ArtistService` j WHERE j.`ServicesId` = s.`Id`);

UPDATE `Services` s
INNER JOIN (
    SELECT `SalonId`, MIN(`Id`) AS `FirstArtistId` FROM `Artists` GROUP BY `SalonId`
) a ON a.`SalonId` = s.`SalonId`
SET s.`CreatedByArtistId` = a.`FirstArtistId`
WHERE s.`CreatedByArtistId` IS NULL;
");

            migrationBuilder.Sql(@"
DELETE FROM `AppointmentServices` WHERE `ServicesId` IN (SELECT `Id` FROM `Services` WHERE `CreatedByArtistId` IS NULL);
DELETE FROM `ArtistService` WHERE `ServicesId` IN (SELECT `Id` FROM `Services` WHERE `CreatedByArtistId` IS NULL);
DELETE FROM `Services` WHERE `CreatedByArtistId` IS NULL;
");

            migrationBuilder.AlterColumn<int>(
                name: "CreatedByArtistId",
                table: "Services",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Services_Artists_CreatedByArtistId",
                table: "Services",
                column: "CreatedByArtistId",
                principalTable: "Artists",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Services_Artists_CreatedByArtistId",
                table: "Services");

            migrationBuilder.AlterColumn<int>(
                name: "CreatedByArtistId",
                table: "Services",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Services_Artists_CreatedByArtistId",
                table: "Services",
                column: "CreatedByArtistId",
                principalTable: "Artists",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
