using Fiap.TechChallenge.OficinaMecanica.Execucao.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fiap.TechChallenge.OficinaMecanica.Execucao.Infrastructure.Data.Configurations;

public sealed class ExecucaoReparoConfiguration : IEntityTypeConfiguration<ExecucaoReparo>
{
    public void Configure(EntityTypeBuilder<ExecucaoReparo> builder)
    {
        builder.ToTable("ExecucaoReparo");
        builder.HasKey(item => item.Id);
        builder.Property(item => item.Status).HasMaxLength(30).IsRequired();
        builder.HasIndex(item => item.OrdemServicoId).IsUnique();
        builder.HasIndex(item => item.CorrelationId);
    }
}
