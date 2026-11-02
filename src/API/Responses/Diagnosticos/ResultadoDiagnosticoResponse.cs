namespace Fiap.TechChallenge.OficinaMecanica.Execucao.API.Responses.Diagnosticos;

public sealed record ResultadoDiagnosticoResponse(
    int DiagnosticoId,
    int OrdemServicoId,
    string ParecerTecnico,
    IReadOnlyCollection<int> ServicoIds,
    IReadOnlyCollection<PecaResultadoDiagnosticoResponse> Pecas,
    DateTimeOffset AtualizadoEm,
    int Versao);

public sealed record PecaResultadoDiagnosticoResponse(int PecaId, int Quantidade);
