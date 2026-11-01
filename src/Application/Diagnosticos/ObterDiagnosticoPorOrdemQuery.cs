using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Execucao.Application.Diagnosticos;

public sealed record ObterDiagnosticoPorOrdemQuery(int OrdemServicoId)
    : IRequest<IniciarDiagnosticoResult>;
