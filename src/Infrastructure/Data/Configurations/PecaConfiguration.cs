using Fiap.TechChallenge.OficinaMecanica.Execucao.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fiap.TechChallenge.OficinaMecanica.Execucao.Infrastructure.Data.Configurations;

public sealed class PecaConfiguration : IEntityTypeConfiguration<Peca>
{
    public void Configure(EntityTypeBuilder<Peca> builder)
    {
        builder.ToTable("Peca");
        builder.HasKey(item => item.Id);
        builder.Property(item => item.Id)
            .UseIdentityByDefaultColumn()
            .HasIdentityOptions(startValue: 1001);
        builder.Property(item => item.Nome).HasMaxLength(150).IsRequired();
        builder.Property(item => item.Marca).HasMaxLength(100).IsRequired();
        builder.Property(item => item.Modelo).HasMaxLength(100).IsRequired();
        builder.Property(item => item.Preco).HasPrecision(18, 2).IsRequired();
        builder.Property(item => item.QuantidadeEstoque).IsRequired();
        builder.Property(item => item.Ativa).IsRequired();
        builder.HasIndex(item => new { item.Marca, item.Modelo }).IsUnique();

        builder.HasData(new
        {
            Id = 1000,
            Nome = "Pastilha de Freio",
            Marca = "Cobreq",
            Modelo = "N-1234",
            Preco = 45m,
            QuantidadeEstoque = 100,
            Ativa = true
        });
    }
}
