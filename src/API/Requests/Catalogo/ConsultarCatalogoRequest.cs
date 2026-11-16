namespace Fiap.TechChallenge.OficinaMecanica.Execucao.API.Requests.Catalogo;

public sealed class ConsultarCatalogoRequest
{
    public IReadOnlyCollection<int>? ServicoIds { get; init; } = [];
    public IReadOnlyCollection<int>? PecaIds { get; init; } = [];
}
