using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgenciaDeToursRD.Migrations
{
    /// <inheritdoc />
    public partial class AgregarColumnaBandera : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Bandera",
                table: "Paises",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Bandera",
                table: "Paises");
        }
    }
}
