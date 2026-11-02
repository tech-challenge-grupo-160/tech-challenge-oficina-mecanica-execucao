using Fiap.TechChallenge.OficinaMecanica.Execucao.Application.Abstractions;
using Fiap.TechChallenge.OficinaMecanica.Execucao.Application.Exceptions;
using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Execucao.Application.Diagnosticos;

public sealed class ObterResultadoDiagnosticoPorOrdemQueryHandler(IDiagnosticoRepository repository)
    : IRequestHandler<ObterResultadoDiagnosticoPorOrdemQuery, ResultadoDiagnosticoResult>
{
    public async Task<ResultadoDiagnosticoResult> Handle(
        ObterResultadoDiagnosticoPorOrdemQuery request,
        CancellationToken cancellationToken)
    {
        var diagnostico = await repository.ObterPorOrdemServicoIdAsync(
            request.OrdemServicoId,
            cancellationToken);
        if (diagnostico is null)
            throw new ServiceNotFoundException(
                $"Diagnostico da ordem de servico {request.OrdemServicoId} nao encontrado.");
        if (diagnostico.ParecerTecnico is null)
            throw new ServiceNotFoundException(
                $"Resultado tecnico da ordem de servico {request.OrdemServicoId} ainda nao registrado.");

        return ResultadoDiagnosticoResult.Criar(diagnostico);
    }
}
