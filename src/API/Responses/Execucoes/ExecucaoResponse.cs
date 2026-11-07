namespace Fiap.TechChallenge.OficinaMecanica.Execucao.API.Responses.Execucoes;

public sealed record ExecucaoResponse(
    Guid Id,
    int OrdemServicoId,
    int DiagnosticoId,
    string Status,
    DateTimeOffset IniciadaEm,
    DateTimeOffset? FinalizadaEm);
