namespace Fiap.TechChallenge.OficinaMecanica.Execucao.API.Requests.Diagnosticos;

public sealed class IniciarDiagnosticoRequest
{
    public int OrdemServicoId { get; init; }
    public int VeiculoId { get; init; }
    public string DescricaoSolicitacao { get; init; } = null!;
    public IReadOnlyCollection<int> ServicoIdsSolicitados { get; init; } = [];
    public IReadOnlyCollection<int> PecaIdsSolicitadas { get; init; } = [];
}
