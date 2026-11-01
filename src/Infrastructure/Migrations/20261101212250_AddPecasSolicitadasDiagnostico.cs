using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fiap.TechChallenge.OficinaMecanica.Execucao.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPecasSolicitadasDiagnostico : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int[]>(
                name: "PecaIdsSolicitadas",
                table: "Diagnostico",
                type: "integer[]",
                nullable: false,
                defaultValue: new int[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PecaIdsSolicitadas",
                table: "Diagnostico");
        }
    }
}
