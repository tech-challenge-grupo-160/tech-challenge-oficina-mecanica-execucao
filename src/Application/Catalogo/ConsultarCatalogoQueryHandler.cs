using Fiap.TechChallenge.OficinaMecanica.Execucao.Application.Abstractions;
using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Execucao.Application.Catalogo;

public sealed class ConsultarCatalogoQueryHandler(ICatalogoRepository repository)
    : IRequestHandler<ConsultarCatalogoQuery, CatalogoResult>
{
    public async Task<CatalogoResult> Handle(
        ConsultarCatalogoQuery request,
        CancellationToken cancellationToken)
    {
        var servicoIds = request.ServicoIds.Distinct().Order().ToArray();
        var pecaIds = request.PecaIds.Distinct().Order().ToArray();

        var servicos = servicoIds.Length == 0
            ? []
            : await repository.ObterServicosAtivosPorIdsAsync(servicoIds, cancellationToken);
        var pecas = pecaIds.Length == 0
            ? []
            : await repository.ObterPecasAtivasPorIdsAsync(pecaIds, cancellationToken);

        var servicosEncontrados = servicos.Select(item => item.Id).ToHashSet();
        var pecasEncontradas = pecas.Select(item => item.Id).ToHashSet();

        return new CatalogoResult(
            servicos
                .OrderBy(item => item.Id)
                .Select(item => new ServicoCatalogoResult(
                    item.Id, item.Nome, item.Preco, item.TempoEstimado))
                .ToArray(),
            pecas
                .OrderBy(item => item.Id)
                .Select(item => new PecaCatalogoResult(item.Id, item.Nome, item.Preco))
                .ToArray(),
            servicoIds.Where(id => !servicosEncontrados.Contains(id)).ToArray(),
            pecaIds.Where(id => !pecasEncontradas.Contains(id)).ToArray());
    }
}
