using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgenciaDeToursRD.Migrations
{
    /// <inheritdoc />
    public partial class AgregarCamposFinales : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Estado",
                table: "Tours",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaFin",
                table: "Tours",
                type: "datetime",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Estado",
                table: "Tours");

            migrationBuilder.DropColumn(
                name: "FechaFin",
                table: "Tours");
        }
    }
}
