namespace Fiap.TechChallenge.OficinaMecanica.Execucao.API.Responses.Estoque;

public sealed record ReposicaoEstoqueResponse(
    int PecaId,
    int QuantidadeReposta,
    int QuantidadeEstoque,
    IReadOnlyCollection<int> OrdensServicoLiberadas);
