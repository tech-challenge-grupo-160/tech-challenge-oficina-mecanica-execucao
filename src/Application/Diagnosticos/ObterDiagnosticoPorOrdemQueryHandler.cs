using Fiap.TechChallenge.OficinaMecanica.Execucao.Application.Abstractions;
using Fiap.TechChallenge.OficinaMecanica.Execucao.Application.Exceptions;
using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Execucao.Application.Diagnosticos;

public sealed class ObterDiagnosticoPorOrdemQueryHandler(IDiagnosticoRepository repository)
    : IRequestHandler<ObterDiagnosticoPorOrdemQuery, IniciarDiagnosticoResult>
{
    public async Task<IniciarDiagnosticoResult> Handle(
        ObterDiagnosticoPorOrdemQuery request,
        CancellationToken cancellationToken)
    {
        var diagnostico = await repository.ObterPorOrdemServicoIdAsync(
            request.OrdemServicoId,
            cancellationToken);
        if (diagnostico is null)
            throw new ServiceNotFoundException(
                $"Diagnostico da ordem de servico {request.OrdemServicoId} nao encontrado.");

        return new IniciarDiagnosticoResult(
            diagnostico.Id,
            diagnostico.OrdemServicoId,
            diagnostico.VeiculoId,
            diagnostico.DescricaoSolicitacao,
            diagnostico.ServicoIdsSolicitados,
            diagnostico.PecaIdsSolicitadas,
            diagnostico.Status,
            diagnostico.IniciadoEm,
            false);
    }
}
