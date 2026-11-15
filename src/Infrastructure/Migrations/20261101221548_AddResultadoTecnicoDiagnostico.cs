using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fiap.TechChallenge.OficinaMecanica.Execucao.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddResultadoTecnicoDiagnostico : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ParecerTecnico",
                table: "Diagnostico",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ResultadoAtualizadoEm",
                table: "Diagnostico",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Versao",
                table: "Diagnostico",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "DiagnosticoPecaResultado",
                columns: table => new
                {
                    DiagnosticoId = table.Column<int>(type: "integer", nullable: false),
                    PecaId = table.Column<int>(type: "integer", nullable: false),
                    Quantidade = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiagnosticoPecaResultado", x => new { x.DiagnosticoId, x.PecaId });
                    table.ForeignKey(
                        name: "FK_DiagnosticoPecaResultado_Diagnostico_DiagnosticoId",
                        column: x => x.DiagnosticoId,
                        principalTable: "Diagnostico",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DiagnosticoServicoResultado",
                columns: table => new
                {
                    DiagnosticoId = table.Column<int>(type: "integer", nullable: false),
                    ServicoId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiagnosticoServicoResultado", x => new { x.DiagnosticoId, x.ServicoId });
                    table.ForeignKey(
                        name: "FK_DiagnosticoServicoResultado_Diagnostico_DiagnosticoId",
                        column: x => x.DiagnosticoId,
                        principalTable: "Diagnostico",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DiagnosticoPecaResultado");

            migrationBuilder.DropTable(
                name: "DiagnosticoServicoResultado");

            migrationBuilder.DropColumn(
                name: "ParecerTecnico",
                table: "Diagnostico");

            migrationBuilder.DropColumn(
                name: "ResultadoAtualizadoEm",
                table: "Diagnostico");

            migrationBuilder.DropColumn(
                name: "Versao",
                table: "Diagnostico");
        }
    }
}
