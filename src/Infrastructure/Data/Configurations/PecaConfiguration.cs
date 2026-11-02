using Fiap.TechChallenge.OficinaMecanica.Execucao.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fiap.TechChallenge.OficinaMecanica.Execucao.Infrastructure.Data.Configurations;

public sealed class PecaConfiguration : IEntityTypeConfiguration<Peca>
{
    private static readonly PecaSeed[] InitialData =
    [
        new(1000, "Pastilha de Freio", "Cobreq", "N-1234", 45m, 100, true),
        new(1001, "Oleo de Motor 5W30 - 5L", "Mobil", "5W30-5L", 189.90m, 40, true),
        new(1002, "Filtro de Oleo", "Mann", "W712-95", 39.90m, 60, true),
        new(1003, "Disco de Freio", "Fremax", "BD-5298", 219.90m, 24, true),
        new(1004, "Correia Dentada", "Gates", "5499XS", 179.90m, 20, true),
        new(1005, "Vela de Ignicao", "NGK", "BKR6E", 42.50m, 80, true),
        new(1006, "Filtro de Ar-Condicionado", "Tecfil", "ACP-887", 49.90m, 35, true),
        new(1007, "Bateria 60Ah", "Moura", "M60GD", 649.90m, 12, true)
    ];

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

        builder.HasData(InitialData);
    }

    private sealed record PecaSeed(
        int Id,
        string Nome,
        string Marca,
        string Modelo,
        decimal Preco,
        int QuantidadeEstoque,
        bool Ativa);
}
