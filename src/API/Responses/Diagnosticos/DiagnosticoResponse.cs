namespace Fiap.TechChallenge.OficinaMecanica.Execucao.API.Responses.Diagnosticos;

public sealed record DiagnosticoResponse(
    int Id,
    int OrdemServicoId,
    int VeiculoId,
    string DescricaoSolicitacao,
    IReadOnlyCollection<int> ServicoIdsSolicitados,
    IReadOnlyCollection<int> PecaIdsSolicitadas,
    string Status,
    DateTimeOffset IniciadoEm);
