using Fiap.TechChallenge.OficinaMecanica.Execucao.Application.Abstractions;
using Fiap.TechChallenge.OficinaMecanica.Execucao.Domain.Entities;
using Fiap.TechChallenge.OficinaMecanica.Execucao.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Fiap.TechChallenge.OficinaMecanica.Execucao.Infrastructure.Repositories;

public sealed class CatalogoRepository(ExecucaoDbContext context) : ICatalogoRepository
{
    public async Task<IReadOnlyList<Servico>> ObterServicosAtivosPorIdsAsync(
        IReadOnlyCollection<int> ids,
        CancellationToken cancellationToken) =>
        await context.Servicos
            .AsNoTracking()
            .Where(item => item.Ativo && ids.Contains(item.Id))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Peca>> ObterPecasAtivasPorIdsAsync(
        IReadOnlyCollection<int> ids,
        CancellationToken cancellationToken) =>
        await context.Pecas
            .AsNoTracking()
            .Where(item => item.Ativa && ids.Contains(item.Id))
            .ToListAsync(cancellationToken);
}
