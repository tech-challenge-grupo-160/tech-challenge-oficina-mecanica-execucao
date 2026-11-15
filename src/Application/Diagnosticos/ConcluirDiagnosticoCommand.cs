using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Execucao.Application.Diagnosticos;

public sealed record ConcluirDiagnosticoCommand(int OrdemServicoId)
    : IRequest<ConclusaoDiagnosticoResult>;
