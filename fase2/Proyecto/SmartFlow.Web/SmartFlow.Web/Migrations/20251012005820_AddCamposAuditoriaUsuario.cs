using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartFlow.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddCamposAuditoriaUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreadoPor",
                table: "Usuarios",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaCreacion",
                table: "Usuarios",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreadoPor",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "FechaCreacion",
                table: "Usuarios");
        }
    }
}
