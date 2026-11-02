using Fiap.TechChallenge.OficinaMecanica.Execucao.Application.Abstractions;
using Fiap.TechChallenge.OficinaMecanica.Execucao.Application.Exceptions;
using Fiap.TechChallenge.OficinaMecanica.Execucao.Domain.Entities;
using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Execucao.Application.Diagnosticos;

public sealed class IniciarDiagnosticoCommandHandler(
    ICatalogoRepository catalogoRepository,
    IDiagnosticoRepository diagnosticoRepository)
    : IRequestHandler<IniciarDiagnosticoCommand, IniciarDiagnosticoResult>
{
    public async Task<IniciarDiagnosticoResult> Handle(
        IniciarDiagnosticoCommand request,
        CancellationToken cancellationToken)
    {
        var servicoIds = request.ServicoIdsSolicitados.Distinct().Order().ToArray();
        var servicos = await catalogoRepository.ObterServicosAtivosPorIdsAsync(servicoIds, cancellationToken);
        var encontrados = servicos.Select(servico => servico.Id).ToHashSet();
        var ausentes = servicoIds.Where(id => !encontrados.Contains(id)).ToArray();
        if (ausentes.Length > 0)
        {
            throw new ServiceNotFoundException(
                $"Servicos solicitados nao encontrados ou inativos: {string.Join(", ", ausentes)}.");
        }

        var pecaIds = request.PecaIdsSolicitadas.Distinct().Order().ToArray();
        var pecas = pecaIds.Length == 0
            ? []
            : await catalogoRepository.ObterPecasAtivasPorIdsAsync(pecaIds, cancellationToken);
        var pecasEncontradas = pecas.Select(peca => peca.Id).ToHashSet();
        var pecasAusentes = pecaIds.Where(id => !pecasEncontradas.Contains(id)).ToArray();
        if (pecasAusentes.Length > 0)
        {
            throw new ServiceNotFoundException(
                $"Pecas solicitadas nao encontradas ou inativas: {string.Join(", ", pecasAusentes)}.");
        }

        var candidato = Diagnostico.Iniciar(
            request.OrdemServicoId,
            request.VeiculoId,
            request.DescricaoSolicitacao,
            servicoIds,
            pecaIds,
            request.ChaveIdempotencia,
            DateTimeOffset.UtcNow,
            request.CorrelationId);

        var (diagnostico, criado) = await diagnosticoRepository.CriarOuObterAsync(
            candidato,
            cancellationToken);

        return new IniciarDiagnosticoResult(
            diagnostico.Id,
            diagnostico.OrdemServicoId,
            diagnostico.VeiculoId,
            diagnostico.DescricaoSolicitacao,
            diagnostico.ServicoIdsSolicitados,
            diagnostico.PecaIdsSolicitadas,
            diagnostico.Status,
            diagnostico.IniciadoEm,
            criado);
    }
}
