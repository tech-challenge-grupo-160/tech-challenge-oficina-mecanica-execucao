using Fiap.TechChallenge.OficinaMecanica.Execucao.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fiap.TechChallenge.OficinaMecanica.Execucao.Infrastructure.Data.Configurations;

public sealed class ServicoConfiguration : IEntityTypeConfiguration<Servico>
{
    private static readonly ServicoSeed[] InitialData =
    [
        new(1000, "Alinhamento", "Alinhamento da geometria das rodas", 150m, 30, true),
        new(1001, "Troca de Oleo", "Troca do oleo do motor e do filtro de oleo", 180m, 45, true),
        new(1002, "Balanceamento", "Balanceamento das quatro rodas", 120m, 30, true),
        new(1003, "Revisao do Sistema de Freios", "Inspecao de pastilhas, discos, fluido e componentes do freio", 250m, 90, true),
        new(1004, "Diagnostico Eletronico", "Leitura de modulos e falhas com scanner automotivo", 200m, 60, true),
        new(1005, "Troca da Correia Dentada", "Substituicao da correia dentada e verificacao dos tensionadores", 650m, 180, true),
        new(1006, "Higienizacao do Ar-Condicionado", "Limpeza do sistema e substituicao do filtro de cabine", 160m, 60, true),
        new(1007, "Revisao Preventiva", "Inspecao geral dos principais itens de seguranca e manutencao", 500m, 150, true)
    ];

    public void Configure(EntityTypeBuilder<Servico> builder)
    {
        builder.ToTable("Servico");
        builder.HasKey(item => item.Id);
        builder.Property(item => item.Id)
            .UseIdentityByDefaultColumn()
            .HasIdentityOptions(startValue: 2000);
        builder.Property(item => item.Nome).HasMaxLength(150).IsRequired();
        builder.Property(item => item.Descricao).HasMaxLength(500).IsRequired();
        builder.Property(item => item.Preco).HasPrecision(18, 2).IsRequired();
        builder.Property(item => item.TempoEstimado).IsRequired();
        builder.Property(item => item.Ativo).IsRequired();
        builder.HasIndex(item => item.Nome);

        builder.HasData(InitialData);
    }

    private sealed record ServicoSeed(
        int Id,
        string Nome,
        string Descricao,
        decimal Preco,
        int TempoEstimado,
        bool Ativo);
}
