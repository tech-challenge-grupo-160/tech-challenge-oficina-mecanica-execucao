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

    [Fact]
    public void ReservarSeDisponivel_DevePreservarSaldoQuandoEstoqueForInsuficiente()
    {
        var peca = Peca.Criar("Pastilha", "Cobreq", "N-1234", 45m, 2);

        peca.ReservarSeDisponivel(4).Should().Be(0);
        peca.QuantidadeEstoque.Should().Be(2);
    }

    [Fact]
    public void ReservarSeDisponivel_DeveBaixarSaldoQuandoEstoqueForSuficiente()
    {
        var peca = Peca.Criar("Pastilha", "Cobreq", "N-1234", 45m, 4);

        peca.ReservarSeDisponivel(3).Should().Be(3);
        peca.QuantidadeEstoque.Should().Be(1);
    }

    [Fact]
    public void Repor_DeveSomarAoEstoqueERejeitarQuantidadeInvalida()
    {
        var peca = Peca.Criar("Pastilha", "Cobreq", "N-1234", 45m, 2);

        peca.Repor(3);
        var reporZero = () => peca.Repor(0);

        peca.QuantidadeEstoque.Should().Be(5);
        reporZero.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ReservarSeDisponivel_DeveRejeitarQuantidadeNaoPositiva(int quantidadeSolicitada)
    {
        var peca = Peca.Criar("Pastilha", "Cobreq", "N-1234", 45m, 4);

        var reservar = () => peca.ReservarSeDisponivel(quantidadeSolicitada);

        reservar.Should().Throw<ArgumentException>();
        peca.QuantidadeEstoque.Should().Be(4);
    }

    [Fact]
    public void ReservaEstoque_DeveIdentificarReservaCompletaEParcial()
    {
        var completa = ReservaEstoque.Criar(
            Guid.NewGuid(), 3007, Guid.NewGuid(),
            [ReservaEstoqueItem.Criar(1000, 2, 2)], DateTimeOffset.UtcNow);
        var parcial = ReservaEstoque.Criar(
            Guid.NewGuid(), 3008, Guid.NewGuid(),
            [ReservaEstoqueItem.Criar(1000, 4, 2)], DateTimeOffset.UtcNow);

        completa.Status.Should().Be("Reservada");
        parcial.Status.Should().Be("EstoqueInsuficiente");
        parcial.Itens.Single().QuantidadeFaltante.Should().Be(2);
        parcial.EventoResultadoId.Should().NotBeEmpty();
    }

    [Fact]
    public void ReservaEstoque_DeveValidarEntradaEReconhecerReprocessamentoCompativel()
    {
        var correlationId = Guid.NewGuid();
        var orcamentoId = Guid.NewGuid();
        var reserva = ReservaEstoque.Criar(
            correlationId, 3007, orcamentoId,
            [ReservaEstoqueItem.Criar(1000, 4, 2)], DateTimeOffset.UtcNow);

        reserva.Corresponde(correlationId, orcamentoId, [(1000, 4)]).Should().BeTrue();
        reserva.Corresponde(Guid.NewGuid(), orcamentoId, [(1000, 4)]).Should().BeFalse();
        reserva.Corresponde(correlationId, Guid.NewGuid(), [(1000, 4)]).Should().BeFalse();
        reserva.Corresponde(correlationId, orcamentoId, [(1000, 3)]).Should().BeFalse();

        var criarSemCorrelationId = () => ReservaEstoque.Criar(
            Guid.Empty, 3007, orcamentoId,
            [ReservaEstoqueItem.Criar(1000, 1, 0)], DateTimeOffset.UtcNow);
        var criarSemOrcamentoId = () => ReservaEstoque.Criar(
            correlationId, 3007, Guid.Empty,
            [ReservaEstoqueItem.Criar(1000, 1, 0)], DateTimeOffset.UtcNow);
        var criarSemItens = () => ReservaEstoque.Criar(
            correlationId, 3007, orcamentoId, [], DateTimeOffset.UtcNow);
        var criarComItemRepetido = () => ReservaEstoque.Criar(
            correlationId, 3007, orcamentoId,
            [ReservaEstoqueItem.Criar(1000, 1, 0), ReservaEstoqueItem.Criar(1000, 2, 0)],
            DateTimeOffset.UtcNow);

        var criarSemOrdemServico = () => ReservaEstoque.Criar(
            correlationId, 0, orcamentoId,
            [ReservaEstoqueItem.Criar(1000, 1, 0)], DateTimeOffset.UtcNow);

        criarSemCorrelationId.Should().Throw<ArgumentException>();
        criarSemOrcamentoId.Should().Throw<ArgumentException>();
        criarSemOrdemServico.Should().Throw<ArgumentException>();
        criarSemItens.Should().Throw<ArgumentException>();
        criarComItemRepetido.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ReservaEstoque_DeveCompletarSomenteQuandoTodasAsFaltasForemAtendidas()
    {
        var reserva = ReservaEstoque.Criar(
            Guid.NewGuid(), 3007, Guid.NewGuid(),
            [
                ReservaEstoqueItem.Criar(1000, 4, 0),
                ReservaEstoqueItem.Criar(1001, 2, 0)
            ],
            DateTimeOffset.UtcNow);
        var eventoInicial = reserva.EventoResultadoId;
        var pecaSemSaldo = Peca.Criar("Pastilha", "Cobreq", "N-1234", 45m, 2);
        var filtro = Peca.Criar("Filtro", "Tecfil", "PSL", 39m, 2);

        reserva.TentarCompletarItem(1000, pecaSemSaldo).Should().BeFalse();
        pecaSemSaldo.Repor(2);
        reserva.TentarCompletarItem(1000, pecaSemSaldo).Should().BeTrue();
        reserva.Status.Should().Be("EstoqueInsuficiente");
        reserva.TentarCompletarItem(1001, filtro).Should().BeTrue();

        reserva.Status.Should().Be("Reservada");
        reserva.EventoResultadoId.Should().NotBe(eventoInicial);
        reserva.Itens.Should().OnlyContain(item => item.QuantidadeFaltante == 0);
        reserva.TentarCompletarItem(1001, filtro).Should().BeFalse();
    }

    [Theory]
    [InlineData(0, 1, 0)]
    [InlineData(1000, 0, 0)]
    [InlineData(1000, 1, -1)]
    [InlineData(1000, 1, 2)]
    public void ReservaEstoqueItem_DeveRejeitarQuantidadesInvalidas(
        int pecaId, int quantidadeSolicitada, int quantidadeReservada)
    {
        var criar = () => ReservaEstoqueItem.Criar(
            pecaId, quantidadeSolicitada, quantidadeReservada);

        criar.Should().Throw<ArgumentException>();
    }
}
