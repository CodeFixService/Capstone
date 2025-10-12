using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartFlow.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddMotivoToSolicitud : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Motivo",
                table: "Solicitudes",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Motivo",
                table: "Solicitudes");
        }
    }
}
