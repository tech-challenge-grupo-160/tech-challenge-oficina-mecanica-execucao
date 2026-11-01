namespace Fiap.TechChallenge.OficinaMecanica.Execucao.API.Responses.Catalogo;

public sealed record CatalogoResponse(
    IReadOnlyList<ServicoCatalogoResponse> Servicos,
    IReadOnlyList<PecaCatalogoResponse> Pecas,
    IReadOnlyList<int> ServicoIdsNaoEncontrados,
    IReadOnlyList<int> PecaIdsNaoEncontrados);

public sealed record ServicoCatalogoResponse(
    int Id, string Nome, decimal Preco, int TempoEstimado);

public sealed record PecaCatalogoResponse(
    int Id, string Nome, decimal Preco);
