using Fiap.TechChallenge.OficinaMecanica.Execucao.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fiap.TechChallenge.OficinaMecanica.Execucao.Infrastructure.Data.Configurations;

public sealed class ServicoConfiguration : IEntityTypeConfiguration<Servico>
{
    public void Configure(EntityTypeBuilder<Servico> builder)
    {
        builder.ToTable("Servico");
        builder.HasKey(item => item.Id);
        builder.Property(item => item.Id)
            .UseIdentityByDefaultColumn()
            .HasIdentityOptions(startValue: 1001);
        builder.Property(item => item.Nome).HasMaxLength(150).IsRequired();
        builder.Property(item => item.Descricao).HasMaxLength(500).IsRequired();
        builder.Property(item => item.Preco).HasPrecision(18, 2).IsRequired();
        builder.Property(item => item.TempoEstimado).IsRequired();
        builder.Property(item => item.Ativo).IsRequired();
        builder.HasIndex(item => item.Nome);

        builder.HasData(new
        {
            Id = 1000,
            Nome = "Alinhamento",
            Descricao = "Servico de alinhamento",
            Preco = 150m,
            TempoEstimado = 30,
            Ativo = true
        });
    }
}
