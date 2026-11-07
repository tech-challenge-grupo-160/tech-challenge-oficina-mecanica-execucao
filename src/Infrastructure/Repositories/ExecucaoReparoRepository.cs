using Fiap.TechChallenge.OficinaMecanica.Execucao.Application.Abstractions;
using Fiap.TechChallenge.OficinaMecanica.Execucao.Domain.Entities;
using Fiap.TechChallenge.OficinaMecanica.Execucao.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Fiap.TechChallenge.OficinaMecanica.Execucao.Infrastructure.Repositories;

public sealed class ExecucaoReparoRepository(ExecucaoDbContext context) : IExecucaoReparoRepository
{
    public async Task<ExecucaoReparo> CriarOuObterAsync(
        Guid correlationId,
        int ordemServicoId,
        int diagnosticoId,
        Guid orcamentoId,
        DateTimeOffset iniciadaEm,
        CancellationToken cancellationToken)
    {
        var existente = await ObterPorOrdemServicoIdAsync(ordemServicoId, cancellationToken);
        if (existente is not null)
        {
            if (!existente.Corresponde(correlationId, diagnosticoId, orcamentoId))
                throw new InvalidOperationException("A ordem de servico possui uma execucao incompativel.");
            return existente;
        }

        var execucao = ExecucaoReparo.Criar(
            correlationId, ordemServicoId, diagnosticoId, orcamentoId, iniciadaEm);
        context.ExecucoesReparo.Add(execucao);
        return execucao;
    }

    public Task<ExecucaoReparo?> ObterPorOrdemServicoIdAsync(
        int ordemServicoId,
        CancellationToken cancellationToken) =>
        context.ExecucoesReparo.SingleOrDefaultAsync(
            item => item.OrdemServicoId == ordemServicoId,
            cancellationToken);

    public async Task<IReadOnlyCollection<ExecucaoReparo>> ListarEmExecucaoAsync(
        CancellationToken cancellationToken) =>
        await context.ExecucoesReparo
            .AsNoTracking()
            .Where(item => item.Status == "EmExecucao")
            .OrderBy(item => item.IniciadaEm)
            .ToArrayAsync(cancellationToken);

    public async Task FinalizarAsync(
        ExecucaoReparo execucao,
        OutboxMessage evento,
        CancellationToken cancellationToken)
    {
        context.OutboxMessages.Add(evento);
        await context.SaveChangesAsync(cancellationToken);
    }
}
