using Fiap.TechChallenge.OficinaMecanica.Execucao.Application.Abstractions;
using Fiap.TechChallenge.OficinaMecanica.Execucao.Application.Exceptions;
using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Execucao.Application.Diagnosticos;

public sealed class RegistrarResultadoDiagnosticoCommandHandler(
    ICatalogoRepository catalogoRepository,
    IDiagnosticoRepository diagnosticoRepository)
    : IRequestHandler<RegistrarResultadoDiagnosticoCommand, ResultadoDiagnosticoResult>
{
    public async Task<ResultadoDiagnosticoResult> Handle(
        RegistrarResultadoDiagnosticoCommand request,
        CancellationToken cancellationToken)
    {
        var diagnostico = await diagnosticoRepository.ObterParaAtualizacaoPorOrdemServicoIdAsync(
            request.OrdemServicoId,
            cancellationToken);
        if (diagnostico is null)
            throw new ServiceNotFoundException(
                $"Diagnostico da ordem de servico {request.OrdemServicoId} nao encontrado.");

        var servicoIds = request.ServicoIds.Order().ToArray();
        var servicos = await catalogoRepository.ObterServicosAtivosPorIdsAsync(servicoIds, cancellationToken);
        var servicosEncontrados = servicos.Select(item => item.Id).ToHashSet();
        var servicosAusentes = servicoIds.Distinct().Where(id => !servicosEncontrados.Contains(id)).ToArray();
        if (servicosAusentes.Length > 0)
            throw new ServiceNotFoundException(
                $"Servicos diagnosticados nao encontrados ou inativos: {string.Join(", ", servicosAusentes)}.");

        var pecas = request.Pecas.OrderBy(item => item.PecaId).ToArray();
        var pecaIds = pecas.Select(item => item.PecaId).Distinct().ToArray();
        var pecasCatalogo = pecaIds.Length == 0
            ? []
            : await catalogoRepository.ObterPecasAtivasPorIdsAsync(pecaIds, cancellationToken);
        var pecasEncontradas = pecasCatalogo.Select(item => item.Id).ToHashSet();
        var pecasAusentes = pecaIds.Where(id => !pecasEncontradas.Contains(id)).ToArray();
        if (pecasAusentes.Length > 0)
            throw new ServiceNotFoundException(
                $"Pecas diagnosticadas nao encontradas ou inativas: {string.Join(", ", pecasAusentes)}.");

        var alterado = diagnostico.RegistrarResultado(
            request.ParecerTecnico,
            servicoIds,
            pecas.Select(item => (item.PecaId, item.Quantidade)),
            DateTimeOffset.UtcNow);

        if (alterado)
            await diagnosticoRepository.AtualizarAsync(diagnostico, cancellationToken);

        return ResultadoDiagnosticoResult.Criar(diagnostico);
    }
}
