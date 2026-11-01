using Fiap.TechChallenge.OficinaMecanica.Execucao.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Fiap.TechChallenge.OficinaMecanica.Execucao.Infrastructure.Data;

public sealed class ExecucaoDbContext(DbContextOptions<ExecucaoDbContext> options)
    : DbContext(options)
{
    public DbSet<Servico> Servicos => Set<Servico>();
    public DbSet<Peca> Pecas => Set<Peca>();
    public DbSet<Diagnostico> Diagnosticos => Set<Diagnostico>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ExecucaoDbContext).Assembly);
    }
}
