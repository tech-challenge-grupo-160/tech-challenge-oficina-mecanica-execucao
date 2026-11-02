using Fiap.TechChallenge.OficinaMecanica.Execucao.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fiap.TechChallenge.OficinaMecanica.Execucao.Infrastructure.Data.Configurations;

public sealed class DiagnosticoServicoResultadoConfiguration
    : IEntityTypeConfiguration<DiagnosticoServicoResultado>
{
    public void Configure(EntityTypeBuilder<DiagnosticoServicoResultado> builder)
    {
        builder.ToTable("DiagnosticoServicoResultado");
        builder.HasKey(item => new { item.DiagnosticoId, item.ServicoId });
        builder.Property(item => item.ServicoId).IsRequired();
    }
}
