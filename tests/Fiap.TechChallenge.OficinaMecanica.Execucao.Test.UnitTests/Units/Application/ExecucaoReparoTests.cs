using Fiap.TechChallenge.OficinaMecanica.Execucao.Application.Abstractions;
using Fiap.TechChallenge.OficinaMecanica.Execucao.Application.Execucoes;
using Fiap.TechChallenge.OficinaMecanica.Execucao.Application.Exceptions;
using Fiap.TechChallenge.OficinaMecanica.Execucao.Domain.Entities;
using FluentAssertions;
using Moq;

namespace Fiap.TechChallenge.OficinaMecanica.Execucao.Test.UnitTests.Units.Application;

public sealed class ExecucaoReparoTests
{
    [Fact]
    public async Task Iniciar_DeveCriarExecucaoComDadosDaSaga()
    {
        var execucao = CriarExecucao();
        var repository = new Mock<IExecucaoReparoRepository>();
        repository.Setup(item => item.CriarOuObterAsync(
                execucao.CorrelationId, 3007, 1, execucao.OrcamentoId,
                execucao.IniciadaEm, It.IsAny<CancellationToken>()))
            .ReturnsAsync(execucao);

        var result = await new IniciarExecucaoCommandHandler(repository.Object).Handle(
            new IniciarExecucaoCommand(
                execucao.CorrelationId, 3007, 1, execucao.OrcamentoId, execucao.IniciadaEm),
            CancellationToken.None);

        result.Status.Should().Be("EmExecucao");
        result.OrdemServicoId.Should().Be(3007);
        repository.VerifyAll();
    }

    [Fact]
    public async Task Consultas_DevemRetornarExecucaoEFila()
    {
        var execucao = CriarExecucao();
        var repository = new Mock<IExecucaoReparoRepository>();
        repository.Setup(item => item.ObterPorOrdemServicoIdAsync(3007, It.IsAny<CancellationToken>()))
            .ReturnsAsync(execucao);
        repository.Setup(item => item.ListarEmExecucaoAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([execucao]);

        var obtida = await new ObterExecucaoPorOrdemQueryHandler(repository.Object)
            .Handle(new ObterExecucaoPorOrdemQuery(3007), CancellationToken.None);
        var fila = await new ListarFilaExecucaoQueryHandler(repository.Object)
            .Handle(new ListarFilaExecucaoQuery(), CancellationToken.None);

        obtida.Id.Should().Be(execucao.Id);
        fila.Should().ContainSingle().Which.Id.Should().Be(execucao.Id);
        repository.VerifyAll();
    }

    [Fact]
    public async Task Obter_DeveInformarQuandoExecucaoNaoExiste()
    {
        var repository = new Mock<IExecucaoReparoRepository>();
        repository.Setup(item => item.ObterPorOrdemServicoIdAsync(9999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ExecucaoReparo?)null);

        var action = () => new ObterExecucaoPorOrdemQueryHandler(repository.Object)
            .Handle(new ObterExecucaoPorOrdemQuery(9999), CancellationToken.None);

        await action.Should().ThrowAsync<ServiceNotFoundException>();
    }

    [Fact]
    public async Task Finalizar_DevePersistirEventoUmaUnicaVez()
    {
        var execucao = CriarExecucao();
        var repository = new Mock<IExecucaoReparoRepository>();
        repository.Setup(item => item.ObterPorOrdemServicoIdAsync(3007, It.IsAny<CancellationToken>()))
            .ReturnsAsync(execucao);
        repository.Setup(item => item.FinalizarAsync(
                execucao,
                It.Is<OutboxMessage>(evento => evento.Tipo == "ExecucaoFinalizadaMessage"),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var handler = new FinalizarExecucaoCommandHandler(repository.Object);

        var primeira = await handler.Handle(new FinalizarExecucaoCommand(3007), CancellationToken.None);
        var repetida = await handler.Handle(new FinalizarExecucaoCommand(3007), CancellationToken.None);

        primeira.Status.Should().Be("Finalizada");
        repetida.FinalizadaEm.Should().Be(primeira.FinalizadaEm);
        repository.Verify(item => item.FinalizarAsync(
            execucao, It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void Dominio_DeveValidarIdentidadeEDetectarMensagemIncompativel()
    {
        var action = () => ExecucaoReparo.Criar(
            Guid.Empty, 0, 0, Guid.Empty, DateTimeOffset.UtcNow);
        var execucao = CriarExecucao();

        action.Should().Throw<ArgumentException>();
        execucao.Corresponde(Guid.NewGuid(), 1, execucao.OrcamentoId).Should().BeFalse();
    }

    [Fact]
    public void Dominio_DeveRejeitarCadaIdentificadorObrigatorioInvalido()
    {
        var correlationId = Guid.NewGuid();
        var orcamentoId = Guid.NewGuid();
        var agora = DateTimeOffset.UtcNow;

        Action ordemInvalida = () => ExecucaoReparo.Criar(correlationId, 0, 1, orcamentoId, agora);
        Action diagnosticoInvalido = () => ExecucaoReparo.Criar(correlationId, 1, 0, orcamentoId, agora);
        Action orcamentoInvalido = () => ExecucaoReparo.Criar(correlationId, 1, 1, Guid.Empty, agora);

        ordemInvalida.Should().Throw<ArgumentException>();
        diagnosticoInvalido.Should().Throw<ArgumentException>();
        orcamentoInvalido.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Validators_DevemRejeitarIdentificadoresInvalidos()
    {
        new IniciarExecucaoCommandValidator().Validate(new IniciarExecucaoCommand(
                Guid.Empty, 0, 0, Guid.Empty, DateTimeOffset.UtcNow))
            .IsValid.Should().BeFalse();
        new ObterExecucaoPorOrdemQueryValidator().Validate(new ObterExecucaoPorOrdemQuery(0))
            .IsValid.Should().BeFalse();
        new FinalizarExecucaoCommandValidator().Validate(new FinalizarExecucaoCommand(0))
            .IsValid.Should().BeFalse();
    }

    private static ExecucaoReparo CriarExecucao() => ExecucaoReparo.Criar(
        Guid.NewGuid(),
        3007,
        1,
        Guid.NewGuid(),
        new DateTimeOffset(2026, 11, 7, 1, 0, 0, TimeSpan.Zero));
}
