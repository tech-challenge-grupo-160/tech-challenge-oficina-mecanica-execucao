using Fiap.TechChallenge.OficinaMecanica.Execucao.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fiap.TechChallenge.OficinaMecanica.Execucao.Infrastructure.Data.Configurations;

public sealed class ReservaEstoqueConfiguration : IEntityTypeConfiguration<ReservaEstoque>
{
    public void Configure(EntityTypeBuilder<ReservaEstoque> builder)
    {
        builder.ToTable("ReservaEstoque");
        builder.HasKey(item => item.Id);
        builder.Property(item => item.Status).HasMaxLength(30).IsRequired();
        builder.Property(item => item.OrcamentoId).IsRequired();
        builder.HasIndex(item => item.OrdemServicoId).IsUnique();
        builder.HasMany(item => item.Itens)
            .WithOne()
            .HasForeignKey(item => item.ReservaEstoqueId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(item => item.Itens)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public sealed class ReservaEstoqueItemConfiguration : IEntityTypeConfiguration<ReservaEstoqueItem>
{
    public void Configure(EntityTypeBuilder<ReservaEstoqueItem> builder)
    {
        builder.ToTable("ReservaEstoqueItem");
        builder.HasKey(item => new { item.ReservaEstoqueId, item.PecaId });
        builder.Ignore(item => item.QuantidadeFaltante);
    }
}
