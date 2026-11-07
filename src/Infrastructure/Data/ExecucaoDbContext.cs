using Fiap.TechChallenge.OficinaMecanica.Execucao.Domain.Entities;
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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ExecucaoDbContext).Assembly);
    }
}
