using Fiap.TechChallenge.OficinaMecanica.Execucao.Domain.Entities;
using Fiap.TechChallenge.OficinaMecanica.Execucao.Infrastructure.Observability;
using Microsoft.EntityFrameworkCore;

namespace Fiap.TechChallenge.OficinaMecanica.Execucao.Infrastructure.Data;

public sealed class ExecucaoDbContext(DbContextOptions<ExecucaoDbContext> options)
    : DbContext(options)
{
    public DbSet<Servico> Servicos => Set<Servico>();
    public DbSet<Peca> Pecas => Set<Peca>();
    public DbSet<Diagnostico> Diagnosticos => Set<Diagnostico>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();
    public DbSet<ReservaEstoque> ReservasEstoque => Set<ReservaEstoque>();
    public DbSet<ReservaEstoqueItem> ReservasEstoqueItens => Set<ReservaEstoqueItem>();
    public DbSet<ExecucaoReparo> ExecucoesReparo => Set<ExecucaoReparo>();

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        RegistrarTraceContext();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        RegistrarTraceContext();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    /// <summary>
    /// Grava o contexto do trace ativo nas mensagens novas da Outbox e da Inbox,
    /// sem cada ponto que cria mensagem precisar lembrar disso (F4-31). A Outbox
    /// usa o contexto para a publicacao continuar o trace de quem gerou a
    /// mensagem; a Inbox guarda o trace da Saga para etapas humanas (concluir
    /// diagnostico, finalizar execucao).
    /// </summary>
    private void RegistrarTraceContext()
    {
        string? contexto = null;
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State != EntityState.Added)
                continue;

            switch (entry.Entity)
            {
                case OutboxMessage { TraceContext: null } outbox:
                    outbox.RegistrarTraceContext(contexto ??= MensageriaTrace.CapturarContextoAtual());
                    break;
                case InboxMessage { TraceContext: null } inbox:
                    inbox.RegistrarTraceContext(contexto ??= MensageriaTrace.CapturarContextoAtual());
                    break;
            }
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ExecucaoDbContext).Assembly);
    }
}
