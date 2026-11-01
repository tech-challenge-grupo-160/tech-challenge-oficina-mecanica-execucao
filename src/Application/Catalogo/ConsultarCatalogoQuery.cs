using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Execucao.Application.Catalogo;

public sealed record ConsultarCatalogoQuery(
    IReadOnlyCollection<int> ServicoIds,
    IReadOnlyCollection<int> PecaIds) : IRequest<CatalogoResult>;

public sealed record CatalogoResult(
    IReadOnlyList<ServicoCatalogoResult> Servicos,
    IReadOnlyList<PecaCatalogoResult> Pecas,
    IReadOnlyList<int> ServicoIdsNaoEncontrados,
    IReadOnlyList<int> PecaIdsNaoEncontrados);

public sealed record ServicoCatalogoResult(
    int Id, string Nome, decimal Preco, int TempoEstimado);

public sealed record PecaCatalogoResult(
    int Id, string Nome, decimal Preco);
