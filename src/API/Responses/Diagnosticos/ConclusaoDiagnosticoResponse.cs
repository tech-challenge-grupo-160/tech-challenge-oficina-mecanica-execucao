namespace Fiap.TechChallenge.OficinaMecanica.Execucao.API.Responses.Diagnosticos;

public sealed record ConclusaoDiagnosticoResponse(
    int DiagnosticoId,
    int OrdemServicoId,
    string Status,
    string ParecerTecnico,
    IReadOnlyCollection<ServicoConclusaoDiagnosticoResponse> Servicos,
    IReadOnlyCollection<PecaConclusaoDiagnosticoResponse> Pecas,
    DateTimeOffset ConcluidoEm);

public sealed record ServicoConclusaoDiagnosticoResponse(
    int ServicoId,
    string Nome,
    decimal Preco,
    int TempoEstimado);

public sealed record PecaConclusaoDiagnosticoResponse(
    int PecaId,
    string Nome,
    int Quantidade,
    decimal Preco);
