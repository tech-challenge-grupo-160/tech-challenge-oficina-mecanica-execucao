using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Fiap.TechChallenge.OficinaMecanica.Execucao.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ExpandInitialCatalogSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Servico",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:IdentitySequenceOptions", "'2000', '1', '', '', 'False', '1'")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:IdentitySequenceOptions", "'1001', '1', '', '', 'False', '1'")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Peca",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:IdentitySequenceOptions", "'2000', '1', '', '', 'False', '1'")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:IdentitySequenceOptions", "'1001', '1', '', '', 'False', '1'")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.InsertData(
                table: "Peca",
                columns: new[] { "Id", "Ativa", "Marca", "Modelo", "Nome", "Preco", "QuantidadeEstoque" },
                values: new object[,]
                {
                    { 1001, true, "Mobil", "5W30-5L", "Oleo de Motor 5W30 - 5L", 189.90m, 40 },
                    { 1002, true, "Mann", "W712-95", "Filtro de Oleo", 39.90m, 60 },
                    { 1003, true, "Fremax", "BD-5298", "Disco de Freio", 219.90m, 24 },
                    { 1004, true, "Gates", "5499XS", "Correia Dentada", 179.90m, 20 },
                    { 1005, true, "NGK", "BKR6E", "Vela de Ignicao", 42.50m, 80 },
                    { 1006, true, "Tecfil", "ACP-887", "Filtro de Ar-Condicionado", 49.90m, 35 },
                    { 1007, true, "Moura", "M60GD", "Bateria 60Ah", 649.90m, 12 }
                });

            migrationBuilder.UpdateData(
                table: "Servico",
                keyColumn: "Id",
                keyValue: 1000,
                column: "Descricao",
                value: "Alinhamento da geometria das rodas");

            migrationBuilder.InsertData(
                table: "Servico",
                columns: new[] { "Id", "Ativo", "Descricao", "Nome", "Preco", "TempoEstimado" },
                values: new object[,]
                {
                    { 1001, true, "Troca do oleo do motor e do filtro de oleo", "Troca de Oleo", 180m, 45 },
                    { 1002, true, "Balanceamento das quatro rodas", "Balanceamento", 120m, 30 },
                    { 1003, true, "Inspecao de pastilhas, discos, fluido e componentes do freio", "Revisao do Sistema de Freios", 250m, 90 },
                    { 1004, true, "Leitura de modulos e falhas com scanner automotivo", "Diagnostico Eletronico", 200m, 60 },
                    { 1005, true, "Substituicao da correia dentada e verificacao dos tensionadores", "Troca da Correia Dentada", 650m, 180 },
                    { 1006, true, "Limpeza do sistema e substituicao do filtro de cabine", "Higienizacao do Ar-Condicionado", 160m, 60 },
                    { 1007, true, "Inspecao geral dos principais itens de seguranca e manutencao", "Revisao Preventiva", 500m, 150 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Peca",
                keyColumn: "Id",
                keyValue: 1001);

            migrationBuilder.DeleteData(
                table: "Peca",
                keyColumn: "Id",
                keyValue: 1002);

            migrationBuilder.DeleteData(
                table: "Peca",
                keyColumn: "Id",
                keyValue: 1003);

            migrationBuilder.DeleteData(
                table: "Peca",
                keyColumn: "Id",
                keyValue: 1004);

            migrationBuilder.DeleteData(
                table: "Peca",
                keyColumn: "Id",
                keyValue: 1005);

            migrationBuilder.DeleteData(
                table: "Peca",
                keyColumn: "Id",
                keyValue: 1006);

            migrationBuilder.DeleteData(
                table: "Peca",
                keyColumn: "Id",
                keyValue: 1007);

            migrationBuilder.DeleteData(
                table: "Servico",
                keyColumn: "Id",
                keyValue: 1001);

            migrationBuilder.DeleteData(
                table: "Servico",
                keyColumn: "Id",
                keyValue: 1002);

            migrationBuilder.DeleteData(
                table: "Servico",
                keyColumn: "Id",
                keyValue: 1003);

            migrationBuilder.DeleteData(
                table: "Servico",
                keyColumn: "Id",
                keyValue: 1004);

            migrationBuilder.DeleteData(
                table: "Servico",
                keyColumn: "Id",
                keyValue: 1005);

            migrationBuilder.DeleteData(
                table: "Servico",
                keyColumn: "Id",
                keyValue: 1006);

            migrationBuilder.DeleteData(
                table: "Servico",
                keyColumn: "Id",
                keyValue: 1007);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Servico",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:IdentitySequenceOptions", "'1001', '1', '', '', 'False', '1'")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:IdentitySequenceOptions", "'2000', '1', '', '', 'False', '1'")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Peca",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:IdentitySequenceOptions", "'1001', '1', '', '', 'False', '1'")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:IdentitySequenceOptions", "'2000', '1', '', '', 'False', '1'")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.UpdateData(
                table: "Servico",
                keyColumn: "Id",
                keyValue: 1000,
                column: "Descricao",
                value: "Servico de alinhamento");
        }
    }
}
