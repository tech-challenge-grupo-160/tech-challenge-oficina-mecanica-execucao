using Fiap.TechChallenge.OficinaMecanica.Execucao.Domain.Entities;
using FluentAssertions;

namespace Fiap.TechChallenge.OficinaMecanica.Execucao.Test.UnitTests.Units.Domain;

public sealed class CatalogoDomainTests
{
    [Fact]
    public void CriarServico_DeveNormalizarDadosEAtivar()
    {
        var servico = Servico.Criar("  Alinhamento  ", "  Alinha as rodas  ", 150m, 30);

        servico.Nome.Should().Be("Alinhamento");
        servico.Descricao.Should().Be("Alinha as rodas");
        servico.Preco.Should().Be(150m);
        servico.TempoEstimado.Should().Be(30);
        servico.Ativo.Should().BeTrue();
    }

    [Theory]
    [InlineData("", "descricao", 10, 30)]
    [InlineData("nome", "", 10, 30)]
    [InlineData("nome", "descricao", -1, 30)]
    [InlineData("nome", "descricao", 10, 0)]
    public void CriarServico_DeveRejeitarDadosInvalidos(
        string nome, string descricao, decimal preco, int tempo)
    {
        var action = () => Servico.Criar(nome, descricao, preco, tempo);

        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void CriarPeca_DevePreservarEstoqueSomenteNoDominioDaExecucao()
    {
        var peca = Peca.Criar("Pastilha", "Cobreq", "N-1234", 45m, 100);

        peca.Preco.Should().Be(45m);
        peca.QuantidadeEstoque.Should().Be(100);
        peca.Ativa.Should().BeTrue();
    }

    [Theory]
    [InlineData("", "marca", "modelo", 10, 1)]
    [InlineData("nome", "", "modelo", 10, 1)]
    [InlineData("nome", "marca", "", 10, 1)]
    [InlineData("nome", "marca", "modelo", -1, 1)]
    [InlineData("nome", "marca", "modelo", 10, -1)]
    public void CriarPeca_DeveRejeitarDadosInvalidos(
        string nome, string marca, string modelo, decimal preco, int estoque)
    {
        var action = () => Peca.Criar(nome, marca, modelo, preco, estoque);

        action.Should().Throw<ArgumentException>();
    }
}
