using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Execucao.Application.Diagnosticos;

public sealed record RegistrarResultadoDiagnosticoCommand(
    int OrdemServicoId,
    string ParecerTecnico,
    IReadOnlyCollection<int> ServicoIds,
    IReadOnlyCollection<PecaResultadoDiagnosticoCommand> Pecas)
    : IRequest<ResultadoDiagnosticoResult>;

public sealed record PecaResultadoDiagnosticoCommand(int PecaId, int Quantidade);
