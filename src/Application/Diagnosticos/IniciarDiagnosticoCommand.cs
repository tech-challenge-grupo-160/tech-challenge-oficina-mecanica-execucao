using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Execucao.Application.Diagnosticos;

public sealed record IniciarDiagnosticoCommand(
    int OrdemServicoId,
    int VeiculoId,
    string DescricaoSolicitacao,
    IReadOnlyCollection<int> ServicoIdsSolicitados,
    IReadOnlyCollection<int> PecaIdsSolicitadas,
    string ChaveIdempotencia) : IRequest<IniciarDiagnosticoResult>;

public sealed record IniciarDiagnosticoResult(
    int Id,
    int OrdemServicoId,
    int VeiculoId,
    string DescricaoSolicitacao,
    IReadOnlyCollection<int> ServicoIdsSolicitados,
    IReadOnlyCollection<int> PecaIdsSolicitadas,
    string Status,
    DateTimeOffset IniciadoEm,
    bool Criado);
