using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartFlow.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddCarrerasRelacionUsuarios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CarreraId",
                table: "Usuarios",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Carreras",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Carreras", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_CarreraId",
                table: "Usuarios",
                column: "CarreraId");

            migrationBuilder.AddForeignKey(
                name: "FK_Usuarios_Carreras_CarreraId",
                table: "Usuarios",
                column: "CarreraId",
                principalTable: "Carreras",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Usuarios_Carreras_CarreraId",
                table: "Usuarios");

            migrationBuilder.DropTable(
                name: "Carreras");

            migrationBuilder.DropIndex(
                name: "IX_Usuarios_CarreraId",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "CarreraId",
                table: "Usuarios");
        }
    }
}
