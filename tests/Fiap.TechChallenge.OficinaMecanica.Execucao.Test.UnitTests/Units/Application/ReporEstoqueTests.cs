using Fiap.TechChallenge.OficinaMecanica.Execucao.Application.Abstractions;
using Fiap.TechChallenge.OficinaMecanica.Execucao.Application.Estoque;
using FluentAssertions;
using Moq;

namespace Fiap.TechChallenge.OficinaMecanica.Execucao.Test.UnitTests.Units.Application;

public sealed class ReporEstoqueTests
{
    [Fact]
    public async Task Handler_DeveRetornarSaldoEOrdensLiberadas()
    {
        var repository = new Mock<IReservaEstoqueRepository>();
        repository.Setup(item => item.ReporAsync(
                1007, 100, It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ReposicaoEstoqueProcessada(1007, 100, 1, [3011]));

        var resultado = await new ReporEstoqueCommandHandler(repository.Object).Handle(
            new ReporEstoqueCommand(1007, 100), CancellationToken.None);

        resultado.PecaId.Should().Be(1007);
        resultado.QuantidadeReposta.Should().Be(100);
        resultado.QuantidadeEstoque.Should().Be(1);
        resultado.OrdensServicoLiberadas.Should().Equal(3011);
        repository.VerifyAll();
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(1000, 0)]
    [InlineData(-1, -1)]
    public void Validator_DeveRejeitarIdentificadorOuQuantidadeInvalidos(
        int pecaId,
        int quantidade)
    {
        var resultado = new ReporEstoqueCommandValidator().Validate(
            new ReporEstoqueCommand(pecaId, quantidade));

        resultado.IsValid.Should().BeFalse();
    }
}
