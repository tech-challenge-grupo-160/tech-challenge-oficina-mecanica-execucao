using Fiap.TechChallenge.OficinaMecanica.Execucao.Domain.Entities;

namespace Fiap.TechChallenge.OficinaMecanica.Execucao.Application.Abstractions;

public interface ICatalogoRepository
{
    Task<IReadOnlyList<Servico>> ObterServicosAtivosPorIdsAsync(
        IReadOnlyCollection<int> ids,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<Peca>> ObterPecasAtivasPorIdsAsync(
        IReadOnlyCollection<int> ids,
        CancellationToken cancellationToken);
}
