using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Execucao.Application.Estoque;

public sealed record ReporEstoqueCommand(int PecaId, int Quantidade)
    : IRequest<ReposicaoEstoqueResult>;

public sealed record ReposicaoEstoqueResult(
    int PecaId,
    int QuantidadeReposta,
    int QuantidadeEstoque,
    IReadOnlyCollection<int> OrdensServicoLiberadas);
