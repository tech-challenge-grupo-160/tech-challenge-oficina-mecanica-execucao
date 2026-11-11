using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fiap.TechChallenge.OficinaMecanica.Execucao.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReposicaoEstoque : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "OrcamentoId",
                table: "ReservaEstoque",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.Sql("""
                UPDATE "ReservaEstoque" AS reserva
                SET "OrcamentoId" = (mensagem."Payload" ->> 'orcamentoId')::uuid
                FROM "OutboxMessage" AS mensagem
                WHERE mensagem."CorrelationId" = reserva."CorrelationId"
                  AND mensagem."Tipo" = 'EstoqueInsuficienteMessage'
                  AND reserva."OrcamentoId" = '00000000-0000-0000-0000-000000000000'
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrcamentoId",
                table: "ReservaEstoque");
        }
    }
}
