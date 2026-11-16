using System.Text.Json;
using Fiap.TechChallenge.OficinaMecanica.Execucao.Application.Abstractions;
using Fiap.TechChallenge.OficinaMecanica.Execucao.Application.Exceptions;
using Fiap.TechChallenge.OficinaMecanica.Execucao.Application.Messaging;
using Fiap.TechChallenge.OficinaMecanica.Execucao.Domain.Entities;
using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Execucao.Application.Execucoes;

public sealed class IniciarExecucaoCommandHandler(IExecucaoReparoRepository repository)
    : IRequestHandler<IniciarExecucaoCommand, ExecucaoResult>
{
    public async Task<ExecucaoResult> Handle(IniciarExecucaoCommand request, CancellationToken cancellationToken) =>
        Mapear(await repository.CriarOuObterAsync(
            request.CorrelationId,
            request.OrdemServicoId,
            request.DiagnosticoId,
            request.OrcamentoId,
            request.IniciadaEm,
            cancellationToken));

    internal static ExecucaoResult Mapear(ExecucaoReparo execucao) => new(
        execucao.Id,
        execucao.CorrelationId,
        execucao.OrdemServicoId,
        execucao.DiagnosticoId,
        execucao.OrcamentoId,
        execucao.Status,
        execucao.IniciadaEm,
        execucao.FinalizadaEm);
}

public sealed class ObterExecucaoPorOrdemQueryHandler(IExecucaoReparoRepository repository)
    : IRequestHandler<ObterExecucaoPorOrdemQuery, ExecucaoResult>
{
    public async Task<ExecucaoResult> Handle(ObterExecucaoPorOrdemQuery request, CancellationToken cancellationToken)
    {
        var execucao = await repository.ObterPorOrdemServicoIdAsync(request.OrdemServicoId, cancellationToken)
            ?? throw new ServiceNotFoundException(
                $"Execucao da ordem de servico {request.OrdemServicoId} nao encontrada.");
        return IniciarExecucaoCommandHandler.Mapear(execucao);
    }
}

public sealed class ListarFilaExecucaoQueryHandler(IExecucaoReparoRepository repository)
    : IRequestHandler<ListarFilaExecucaoQuery, IReadOnlyCollection<ExecucaoResult>>
{
    public async Task<IReadOnlyCollection<ExecucaoResult>> Handle(
        ListarFilaExecucaoQuery request,
        CancellationToken cancellationToken) =>
        (await repository.ListarEmExecucaoAsync(cancellationToken))
            .Select(IniciarExecucaoCommandHandler.Mapear)
            .ToArray();
}

public sealed class FinalizarExecucaoCommandHandler(IExecucaoReparoRepository repository)
    : IRequestHandler<FinalizarExecucaoCommand, ExecucaoResult>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<ExecucaoResult> Handle(FinalizarExecucaoCommand request, CancellationToken cancellationToken)
    {
        var execucao = await repository.ObterPorOrdemServicoIdAsync(request.OrdemServicoId, cancellationToken)
            ?? throw new ServiceNotFoundException(
                $"Execucao da ordem de servico {request.OrdemServicoId} nao encontrada.");
        if (!execucao.Finalizar(DateTimeOffset.UtcNow))
            return IniciarExecucaoCommandHandler.Mapear(execucao);

        var evento = new ExecucaoFinalizadaMessage(
            execucao.EventoFinalizacaoId!.Value,
            execucao.CorrelationId,
            execucao.FinalizadaEm!.Value,
            execucao.OrdemServicoId,
            execucao.DiagnosticoId,
            execucao.OrcamentoId,
            execucao.Status,
            execucao.FinalizadaEm.Value);
        var outbox = OutboxMessage.Criar(
            evento.MessageId,
            evento.CorrelationId,
            nameof(ExecucaoFinalizadaMessage),
            "OficinaEventos",
            JsonSerializer.Serialize(evento, JsonOptions),
            evento.OcorridoEm);
        await repository.FinalizarAsync(execucao, outbox, cancellationToken);
        return IniciarExecucaoCommandHandler.Mapear(execucao);
    }
}
