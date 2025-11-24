using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartFlow.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddCamposChatMensaje : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AdminId",
                table: "ChatMensajes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CarreraId",
                table: "ChatMensajes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CoordinadorId",
                table: "ChatMensajes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DestinatarioRol",
                table: "ChatMensajes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "LeidoPorCoordinador",
                table: "ChatMensajes",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdminId",
                table: "ChatMensajes");

            migrationBuilder.DropColumn(
                name: "CarreraId",
                table: "ChatMensajes");

            migrationBuilder.DropColumn(
                name: "CoordinadorId",
                table: "ChatMensajes");

            migrationBuilder.DropColumn(
                name: "DestinatarioRol",
                table: "ChatMensajes");

            migrationBuilder.DropColumn(
                name: "LeidoPorCoordinador",
                table: "ChatMensajes");
        }
    }
}
