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
            .HasIdentityOptions(startValue: 2000);
        builder.Property(item => item.Nome).HasMaxLength(150).IsRequired();
        builder.Property(item => item.Marca).HasMaxLength(100).IsRequired();
        builder.Property(item => item.Modelo).HasMaxLength(100).IsRequired();
        builder.Property(item => item.Preco).HasPrecision(18, 2).IsRequired();
        builder.Property(item => item.QuantidadeEstoque).IsRequired();
        builder.Property(item => item.Ativa).IsRequired();
        builder.HasIndex(item => new { item.Marca, item.Modelo }).IsUnique();

        builder.HasData(
            new
            {
                Id = 1000,
                Nome = "Pastilha de Freio",
                Marca = "Cobreq",
                Modelo = "N-1234",
                Preco = 45m,
                QuantidadeEstoque = 100,
                Ativa = true
            },
            new
            {
                Id = 1001,
                Nome = "Oleo de Motor 5W30 - 5L",
                Marca = "Mobil",
                Modelo = "5W30-5L",
                Preco = 189.90m,
                QuantidadeEstoque = 40,
                Ativa = true
            },
            new
            {
                Id = 1002,
                Nome = "Filtro de Oleo",
                Marca = "Mann",
                Modelo = "W712-95",
                Preco = 39.90m,
                QuantidadeEstoque = 60,
                Ativa = true
            },
            new
            {
                Id = 1003,
                Nome = "Disco de Freio",
                Marca = "Fremax",
                Modelo = "BD-5298",
                Preco = 219.90m,
                QuantidadeEstoque = 24,
                Ativa = true
            },
            new
            {
                Id = 1004,
                Nome = "Correia Dentada",
                Marca = "Gates",
                Modelo = "5499XS",
                Preco = 179.90m,
                QuantidadeEstoque = 20,
                Ativa = true
            },
            new
            {
                Id = 1005,
                Nome = "Vela de Ignicao",
                Marca = "NGK",
                Modelo = "BKR6E",
                Preco = 42.50m,
                QuantidadeEstoque = 80,
                Ativa = true
            },
            new
            {
                Id = 1006,
                Nome = "Filtro de Ar-Condicionado",
                Marca = "Tecfil",
                Modelo = "ACP-887",
                Preco = 49.90m,
                QuantidadeEstoque = 35,
                Ativa = true
            },
            new
            {
                Id = 1007,
                Nome = "Bateria 60Ah",
                Marca = "Moura",
                Modelo = "M60GD",
                Preco = 649.90m,
                QuantidadeEstoque = 12,
                Ativa = true
            });
    }
}
