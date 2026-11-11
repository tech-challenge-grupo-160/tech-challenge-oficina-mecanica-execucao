using Fiap.TechChallenge.OficinaMecanica.Execucao.Application.Abstractions;
using Fiap.TechChallenge.OficinaMecanica.Execucao.Application.Estoque;
using Fiap.TechChallenge.OficinaMecanica.Execucao.Domain.Entities;
using FluentAssertions;
using Moq;

namespace Fiap.TechChallenge.OficinaMecanica.Execucao.Test.UnitTests.Units.Application;

public sealed class ReservarPecasTests
{
    [Fact]
    public async Task Handler_DeveRetornarQuantidadesReservadasEFaltantes()
    {
        var correlationId = Guid.NewGuid();
        var orcamentoId = Guid.NewGuid();
        var reserva = ReservaEstoque.Criar(
            correlationId, 3007, orcamentoId,
            [ReservaEstoqueItem.Criar(1000, 4, 2)], DateTimeOffset.UtcNow);
        var repository = new Mock<IReservaEstoqueRepository>();
        repository.Setup(item => item.ReservarAsync(
                correlationId, 3007, orcamentoId,
                It.Is<IReadOnlyCollection<(int PecaId, int Quantidade)>>(pecas =>
                    pecas.Single().PecaId == 1000 && pecas.Single().Quantidade == 4),
                It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(reserva);

        var result = await new ReservarPecasCommandHandler(repository.Object)
            .Handle(new ReservarPecasCommand(
                correlationId, 3007, orcamentoId, [new PecaReservaCommand(1000, 4)]),
                CancellationToken.None);

        result.Status.Should().Be("EstoqueInsuficiente");
        result.Pecas.Single().QuantidadeReservada.Should().Be(2);
        result.Pecas.Single().QuantidadeFaltante.Should().Be(2);
        repository.VerifyAll();
    }

    [Fact]
    public void Validator_DeveRejeitarPecasDuplicadasOuQuantidadeInvalida()
    {
        var validator = new ReservarPecasCommandValidator();
        var result = validator.Validate(new ReservarPecasCommand(
            Guid.NewGuid(), 3007, Guid.NewGuid(),
            [new PecaReservaCommand(1000, 1), new PecaReservaCommand(1000, 0)]));

        result.IsValid.Should().BeFalse();
    }
}
