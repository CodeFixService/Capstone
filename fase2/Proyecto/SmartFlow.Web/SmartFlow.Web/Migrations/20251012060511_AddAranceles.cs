using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartFlow.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddAranceles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Aranceles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Monto = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CarreraId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Aranceles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Aranceles_Carreras_CarreraId",
                        column: x => x.CarreraId,
                        principalTable: "Carreras",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Aranceles_CarreraId",
                table: "Aranceles",
                column: "CarreraId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Aranceles");
        }
    }
}
