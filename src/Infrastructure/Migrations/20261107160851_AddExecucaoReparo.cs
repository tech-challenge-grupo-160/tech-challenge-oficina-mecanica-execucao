using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fiap.TechChallenge.OficinaMecanica.Execucao.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddExecucaoReparo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExecucaoReparo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrdemServicoId = table.Column<int>(type: "integer", nullable: false),
                    DiagnosticoId = table.Column<int>(type: "integer", nullable: false),
                    OrcamentoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    IniciadaEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    FinalizadaEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    EventoFinalizacaoId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExecucaoReparo", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExecucaoReparo_CorrelationId",
                table: "ExecucaoReparo",
                column: "CorrelationId");

            migrationBuilder.CreateIndex(
                name: "IX_ExecucaoReparo_OrdemServicoId",
                table: "ExecucaoReparo",
                column: "OrdemServicoId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExecucaoReparo");
        }
    }
}
