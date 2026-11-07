using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fiap.TechChallenge.OficinaMecanica.Execucao.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReservaEstoque : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReservaEstoque",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventoResultadoId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrdemServicoId = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CriadaEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReservaEstoque", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReservaEstoqueItem",
                columns: table => new
                {
                    ReservaEstoqueId = table.Column<Guid>(type: "uuid", nullable: false),
                    PecaId = table.Column<int>(type: "integer", nullable: false),
                    QuantidadeSolicitada = table.Column<int>(type: "integer", nullable: false),
                    QuantidadeReservada = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReservaEstoqueItem", x => new { x.ReservaEstoqueId, x.PecaId });
                    table.ForeignKey(
                        name: "FK_ReservaEstoqueItem_ReservaEstoque_ReservaEstoqueId",
                        column: x => x.ReservaEstoqueId,
                        principalTable: "ReservaEstoque",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReservaEstoque_OrdemServicoId",
                table: "ReservaEstoque",
                column: "OrdemServicoId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReservaEstoqueItem");

            migrationBuilder.DropTable(
                name: "ReservaEstoque");
        }
    }
}
