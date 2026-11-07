using Fiap.TechChallenge.OficinaMecanica.Execucao.Domain.Entities;

namespace Fiap.TechChallenge.OficinaMecanica.Execucao.Application.Abstractions;

public interface IExecucaoReparoRepository
{
    Task<ExecucaoReparo> CriarOuObterAsync(
        Guid correlationId,
        int ordemServicoId,
        int diagnosticoId,
        Guid orcamentoId,
        DateTimeOffset iniciadaEm,
        CancellationToken cancellationToken);

    Task<ExecucaoReparo?> ObterPorOrdemServicoIdAsync(
        int ordemServicoId,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<ExecucaoReparo>> ListarEmExecucaoAsync(
        CancellationToken cancellationToken);

    Task FinalizarAsync(
        ExecucaoReparo execucao,
        OutboxMessage evento,
        CancellationToken cancellationToken);
}
