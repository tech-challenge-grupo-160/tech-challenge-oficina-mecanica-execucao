using Fiap.TechChallenge.OficinaMecanica.Execucao.Domain.Entities;

namespace Fiap.TechChallenge.OficinaMecanica.Execucao.Application.Abstractions;

public interface IReservaEstoqueRepository
{
    Task<ReservaEstoque> ReservarAsync(
        Guid correlationId,
        int ordemServicoId,
        IReadOnlyCollection<(int PecaId, int Quantidade)> itens,
        DateTimeOffset criadaEm,
        CancellationToken cancellationToken);
}
