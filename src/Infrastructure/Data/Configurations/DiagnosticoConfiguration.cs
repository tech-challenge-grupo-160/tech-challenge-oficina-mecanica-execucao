using Fiap.TechChallenge.OficinaMecanica.Execucao.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fiap.TechChallenge.OficinaMecanica.Execucao.Infrastructure.Data.Configurations;

public sealed class DiagnosticoConfiguration : IEntityTypeConfiguration<Diagnostico>
{
    public void Configure(EntityTypeBuilder<Diagnostico> builder)
    {
        builder.ToTable("Diagnostico");
        builder.HasKey(diagnostico => diagnostico.Id);
        builder.Property(diagnostico => diagnostico.Id).UseIdentityByDefaultColumn();
        builder.Property(diagnostico => diagnostico.OrdemServicoId).IsRequired();
        builder.Property(diagnostico => diagnostico.VeiculoId).IsRequired();
        builder.Property(diagnostico => diagnostico.DescricaoSolicitacao)
            .HasMaxLength(1000)
            .IsRequired();
        builder.Property(diagnostico => diagnostico.ServicoIdsSolicitados)
            .HasColumnType("integer[]")
            .IsRequired();
        builder.Property(diagnostico => diagnostico.PecaIdsSolicitadas)
            .HasColumnType("integer[]")
            .IsRequired();
        builder.Property(diagnostico => diagnostico.ChaveIdempotencia)
            .HasMaxLength(100)
            .IsRequired();
        builder.Property(diagnostico => diagnostico.Status)
            .HasMaxLength(50)
            .IsRequired();
        builder.Property(diagnostico => diagnostico.IniciadoEm).IsRequired();
        builder.HasIndex(diagnostico => diagnostico.OrdemServicoId).IsUnique();
        builder.HasIndex(diagnostico => diagnostico.ChaveIdempotencia).IsUnique();
    }
}
