using Fiap.TechChallenge.OficinaMecanica.Execucao.Application.Abstractions;
using Fiap.TechChallenge.OficinaMecanica.Execucao.Domain.Entities;
using Fiap.TechChallenge.OficinaMecanica.Execucao.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Fiap.TechChallenge.OficinaMecanica.Execucao.Infrastructure.Repositories;

public sealed class ReservaEstoqueRepository(ExecucaoDbContext context)
    : IReservaEstoqueRepository
{
    public async Task<ReservaEstoque> ReservarAsync(
        Guid correlationId,
        int ordemServicoId,
        IReadOnlyCollection<(int PecaId, int Quantidade)> itens,
        DateTimeOffset criadaEm,
        CancellationToken cancellationToken)
    {
        var existente = await context.ReservasEstoque
            .Include(reserva => reserva.Itens)
            .SingleOrDefaultAsync(
                reserva => reserva.OrdemServicoId == ordemServicoId,
                cancellationToken);
        if (existente is not null)
        {
            if (!existente.Corresponde(correlationId, itens))
                throw new InvalidOperationException(
                    "A ordem de servico ja possui uma reserva de estoque incompatível.");
            return existente;
        }

        var ids = itens.Select(item => item.PecaId).Distinct().ToArray();
        var pecas = await context.Pecas
            .Where(peca => peca.Ativa && ids.Contains(peca.Id))
            .ToDictionaryAsync(peca => peca.Id, cancellationToken);
        var resultados = itens.Select(item =>
        {
            var reservada = pecas.TryGetValue(item.PecaId, out var peca)
                ? peca.ReservarSeDisponivel(item.Quantidade)
                : 0;
            return ReservaEstoqueItem.Criar(item.PecaId, item.Quantidade, reservada);
        }).ToArray();

        var reserva = ReservaEstoque.Criar(
            correlationId,
            ordemServicoId,
            resultados,
            criadaEm);
        context.ReservasEstoque.Add(reserva);
        return reserva;
    }
}
