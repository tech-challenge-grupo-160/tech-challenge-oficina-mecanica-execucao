using Fiap.TechChallenge.OficinaMecanica.Execucao.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fiap.TechChallenge.OficinaMecanica.Execucao.Infrastructure.Data.Configurations;

public sealed class DiagnosticoPecaResultadoConfiguration
    : IEntityTypeConfiguration<DiagnosticoPecaResultado>
{
    public void Configure(EntityTypeBuilder<DiagnosticoPecaResultado> builder)
    {
        builder.ToTable("DiagnosticoPecaResultado");
        builder.HasKey(item => new { item.DiagnosticoId, item.PecaId });
        builder.Property(item => item.PecaId).IsRequired();
        builder.Property(item => item.Quantidade).IsRequired();
    }
}
