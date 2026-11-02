namespace Fiap.TechChallenge.OficinaMecanica.Execucao.API.Requests.Diagnosticos;

public sealed class RegistrarResultadoDiagnosticoRequest
{
    public string ParecerTecnico { get; init; } = null!;
    public IReadOnlyCollection<int> ServicoIds { get; init; } = [];
    public IReadOnlyCollection<PecaResultadoDiagnosticoRequest>? Pecas { get; init; } = [];
}

public sealed class PecaResultadoDiagnosticoRequest
{
    public int PecaId { get; init; }
    public int Quantidade { get; init; }
}
