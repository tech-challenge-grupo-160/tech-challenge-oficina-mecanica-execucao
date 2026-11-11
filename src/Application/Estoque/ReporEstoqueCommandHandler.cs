using Fiap.TechChallenge.OficinaMecanica.Execucao.Application.Abstractions;
using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Execucao.Application.Estoque;

public sealed class ReporEstoqueCommandHandler(IReservaEstoqueRepository repository)
    : IRequestHandler<ReporEstoqueCommand, ReposicaoEstoqueResult>
{
    public async Task<ReposicaoEstoqueResult> Handle(
        ReporEstoqueCommand request,
        CancellationToken cancellationToken)
    {
        var resultado = await repository.ReporAsync(
            request.PecaId,
            request.Quantidade,
            DateTimeOffset.UtcNow,
            cancellationToken);
        return new ReposicaoEstoqueResult(
            resultado.PecaId,
            resultado.QuantidadeReposta,
            resultado.QuantidadeEstoque,
            resultado.OrdensServicoLiberadas);
    }
}
