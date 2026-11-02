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
            .HasIdentityOptions(startValue: 2000);
        builder.Property(item => item.Nome).HasMaxLength(150).IsRequired();
        builder.Property(item => item.Descricao).HasMaxLength(500).IsRequired();
        builder.Property(item => item.Preco).HasPrecision(18, 2).IsRequired();
        builder.Property(item => item.TempoEstimado).IsRequired();
        builder.Property(item => item.Ativo).IsRequired();
        builder.HasIndex(item => item.Nome);

        builder.HasData(
            new
            {
                Id = 1000,
                Nome = "Alinhamento",
                Descricao = "Alinhamento da geometria das rodas",
                Preco = 150m,
                TempoEstimado = 30,
                Ativo = true
            },
            new
            {
                Id = 1001,
                Nome = "Troca de Oleo",
                Descricao = "Troca do oleo do motor e do filtro de oleo",
                Preco = 180m,
                TempoEstimado = 45,
                Ativo = true
            },
            new
            {
                Id = 1002,
                Nome = "Balanceamento",
                Descricao = "Balanceamento das quatro rodas",
                Preco = 120m,
                TempoEstimado = 30,
                Ativo = true
            },
            new
            {
                Id = 1003,
                Nome = "Revisao do Sistema de Freios",
                Descricao = "Inspecao de pastilhas, discos, fluido e componentes do freio",
                Preco = 250m,
                TempoEstimado = 90,
                Ativo = true
            },
            new
            {
                Id = 1004,
                Nome = "Diagnostico Eletronico",
                Descricao = "Leitura de modulos e falhas com scanner automotivo",
                Preco = 200m,
                TempoEstimado = 60,
                Ativo = true
            },
            new
            {
                Id = 1005,
                Nome = "Troca da Correia Dentada",
                Descricao = "Substituicao da correia dentada e verificacao dos tensionadores",
                Preco = 650m,
                TempoEstimado = 180,
                Ativo = true
            },
            new
            {
                Id = 1006,
                Nome = "Higienizacao do Ar-Condicionado",
                Descricao = "Limpeza do sistema e substituicao do filtro de cabine",
                Preco = 160m,
                TempoEstimado = 60,
                Ativo = true
            },
            new
            {
                Id = 1007,
                Nome = "Revisao Preventiva",
                Descricao = "Inspecao geral dos principais itens de seguranca e manutencao",
                Preco = 500m,
                TempoEstimado = 150,
                Ativo = true
            });
    }
}
