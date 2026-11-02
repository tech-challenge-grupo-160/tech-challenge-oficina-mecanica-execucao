using Fiap.TechChallenge.OficinaMecanica.Execucao.Application.Abstractions;
using Fiap.TechChallenge.OficinaMecanica.Execucao.Application.Exceptions;
using Fiap.TechChallenge.OficinaMecanica.Execucao.Domain.Entities;
using Fiap.TechChallenge.OficinaMecanica.Execucao.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Fiap.TechChallenge.OficinaMecanica.Execucao.Infrastructure.Repositories;

public sealed class DiagnosticoRepository(ExecucaoDbContext context) : IDiagnosticoRepository
{
    public async Task<(Diagnostico Diagnostico, bool Criado)> CriarOuObterAsync(
        Diagnostico diagnostico,
        CancellationToken cancellationToken)
    {
        var existente = await ObterExistenteAsync(diagnostico, cancellationToken);
        if (existente is not null)
            return ResolverRepeticao(existente, diagnostico);

        context.Diagnosticos.Add(diagnostico);
        try
        {
            await context.SaveChangesAsync(cancellationToken);
            return (diagnostico, true);
        }
        catch (DbUpdateException exception)
            when (exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            context.ChangeTracker.Clear();
            existente = await ObterExistenteAsync(diagnostico, cancellationToken);
            if (existente is null)
                throw;

            return ResolverRepeticao(existente, diagnostico);
        }
    }

    public Task<Diagnostico?> ObterPorOrdemServicoIdAsync(
        int ordemServicoId,
        CancellationToken cancellationToken) =>
        context.Diagnosticos
            .AsNoTracking()
            .AsSplitQuery()
            .Include(diagnostico => diagnostico.ServicosResultado)
            .Include(diagnostico => diagnostico.PecasResultado)
            .FirstOrDefaultAsync(
                diagnostico => diagnostico.OrdemServicoId == ordemServicoId,
                cancellationToken);

    public Task<Diagnostico?> ObterParaAtualizacaoPorOrdemServicoIdAsync(
        int ordemServicoId,
        CancellationToken cancellationToken) =>
        context.Diagnosticos
            .AsSplitQuery()
            .Include(diagnostico => diagnostico.ServicosResultado)
            .Include(diagnostico => diagnostico.PecasResultado)
            .FirstOrDefaultAsync(
                diagnostico => diagnostico.OrdemServicoId == ordemServicoId,
                cancellationToken);

    public async Task AtualizarAsync(
        Diagnostico diagnostico,
        CancellationToken cancellationToken)
    {
        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ServiceConflictException(
                "O resultado do diagnostico foi alterado por outra requisicao. Consulte-o e tente novamente.");
        }
    }

    public async Task ConcluirAsync(
        Diagnostico diagnostico,
        OutboxMessage evento,
        CancellationToken cancellationToken)
    {
        context.OutboxMessages.Add(evento);
        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ServiceConflictException(
                "O diagnostico foi alterado por outra requisicao. Consulte-o e tente novamente.");
        }
    }

    private async Task<Diagnostico?> ObterExistenteAsync(
        Diagnostico diagnostico,
        CancellationToken cancellationToken)
    {
        var existentes = await context.Diagnosticos
            .AsNoTracking()
            .Where(item => item.OrdemServicoId == diagnostico.OrdemServicoId ||
                           item.ChaveIdempotencia == diagnostico.ChaveIdempotencia)
            .OrderBy(item => item.Id)
            .Take(2)
            .ToListAsync(cancellationToken);

        if (existentes.Count > 1)
        {
            throw new ServiceConflictException(
                "A ordem de servico e a chave de idempotencia pertencem a diagnosticos diferentes.");
        }

        return existentes.FirstOrDefault();
    }

    private static (Diagnostico Diagnostico, bool Criado) ResolverRepeticao(
        Diagnostico existente,
        Diagnostico candidato)
    {
        if (!existente.CorrespondeA(candidato))
        {
            throw new ServiceConflictException(
                "A chave de idempotencia ou a ordem de servico ja foi usada com dados diferentes.");
        }

        return (existente, false);
    }
}
