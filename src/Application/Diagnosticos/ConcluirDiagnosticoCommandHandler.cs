using System.Text.Json;
using Fiap.TechChallenge.OficinaMecanica.Execucao.Application.Abstractions;
using Fiap.TechChallenge.OficinaMecanica.Execucao.Application.Exceptions;
using Fiap.TechChallenge.OficinaMecanica.Execucao.Application.Messaging;
using Fiap.TechChallenge.OficinaMecanica.Execucao.Domain.Entities;
using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Execucao.Application.Diagnosticos;

public sealed class ConcluirDiagnosticoCommandHandler(
    ICatalogoRepository catalogoRepository,
    IDiagnosticoRepository diagnosticoRepository)
    : IRequestHandler<ConcluirDiagnosticoCommand, ConclusaoDiagnosticoResult>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<ConclusaoDiagnosticoResult> Handle(
        ConcluirDiagnosticoCommand request,
        CancellationToken cancellationToken)
    {
        var diagnostico = await diagnosticoRepository.ObterParaAtualizacaoPorOrdemServicoIdAsync(
            request.OrdemServicoId,
            cancellationToken);
        if (diagnostico is null)
            throw new ServiceNotFoundException(
                $"Diagnostico da ordem de servico {request.OrdemServicoId} nao encontrado.");

        var alterado = diagnostico.Concluir(DateTimeOffset.UtcNow);
        var servicos = await ObterServicosAsync(diagnostico, cancellationToken);
        var pecas = await ObterPecasAsync(diagnostico, cancellationToken);
        var resultado = ConclusaoDiagnosticoResult.Criar(diagnostico, servicos, pecas, alterado);

        if (!alterado)
            return resultado;

        var evento = new DiagnosticoConcluidoMessage(
            Guid.NewGuid(),
            diagnostico.CorrelationId,
            resultado.ConcluidoEm,
            resultado.OrdemServicoId,
            resultado.DiagnosticoId,
            resultado.Status,
            resultado.ParecerTecnico,
            resultado.Servicos.Select(item => new ServicoDiagnosticoConcluidoMessage(
                item.ServicoId,
                item.Nome,
                item.Preco,
                item.TempoEstimado)).ToArray(),
            resultado.Pecas.Select(item => new PecaDiagnosticoConcluidoMessage(
                item.PecaId,
                item.Nome,
                item.Quantidade,
                item.Preco)).ToArray(),
            resultado.ConcluidoEm);
        var outbox = OutboxMessage.Criar(
            evento.MessageId,
            evento.CorrelationId,
            nameof(DiagnosticoConcluidoMessage),
            "OficinaEventos",
            JsonSerializer.Serialize(evento, JsonOptions),
            evento.OcorridoEm);

        await diagnosticoRepository.ConcluirAsync(diagnostico, outbox, cancellationToken);
        return resultado;
    }

    private async Task<IReadOnlyDictionary<int, Servico>> ObterServicosAsync(
        Diagnostico diagnostico,
        CancellationToken cancellationToken)
    {
        var ids = diagnostico.ServicosResultado.Select(item => item.ServicoId).ToArray();
        var itens = await catalogoRepository.ObterServicosAtivosPorIdsAsync(ids, cancellationToken);
        if (itens.Count != ids.Length)
            throw new ServiceConflictException(
                "Um ou mais servicos do resultado tecnico nao estao disponiveis no catalogo.");
        return itens.ToDictionary(item => item.Id);
    }

    private async Task<IReadOnlyDictionary<int, Peca>> ObterPecasAsync(
        Diagnostico diagnostico,
        CancellationToken cancellationToken)
    {
        var ids = diagnostico.PecasResultado.Select(item => item.PecaId).ToArray();
        if (ids.Length == 0)
            return new Dictionary<int, Peca>();

        var itens = await catalogoRepository.ObterPecasAtivasPorIdsAsync(ids, cancellationToken);
        if (itens.Count != ids.Length)
            throw new ServiceConflictException(
                "Uma ou mais pecas do resultado tecnico nao estao disponiveis no catalogo.");
        return itens.ToDictionary(item => item.Id);
    }
}
