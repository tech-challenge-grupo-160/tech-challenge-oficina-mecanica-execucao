using Fiap.TechChallenge.OficinaMecanica.Execucao.Domain.Entities;

namespace Fiap.TechChallenge.OficinaMecanica.Execucao.Application.Abstractions;

public interface IReservaEstoqueRepository
{
    Task<ReservaEstoque> ReservarAsync(
        Guid correlationId,
        int ordemServicoId,
        Guid orcamentoId,
        IReadOnlyCollection<(int PecaId, int Quantidade)> itens,
        DateTimeOffset criadaEm,
        CancellationToken cancellationToken);

    Task<ReposicaoEstoqueProcessada> ReporAsync(
        int pecaId,
        int quantidade,
        DateTimeOffset repostaEm,
        CancellationToken cancellationToken);
}

public sealed record ReposicaoEstoqueProcessada(
    int PecaId,
    int QuantidadeReposta,
    int QuantidadeEstoque,
    IReadOnlyCollection<int> OrdensServicoLiberadas);
