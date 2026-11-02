using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Execucao.Application.Diagnosticos;

public sealed record ObterResultadoDiagnosticoPorOrdemQuery(int OrdemServicoId)
    : IRequest<ResultadoDiagnosticoResult>;
