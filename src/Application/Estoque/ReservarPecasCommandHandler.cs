using Fiap.TechChallenge.OficinaMecanica.Execucao.Application.Abstractions;
using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Execucao.Application.Estoque;

public sealed class ReservarPecasCommandHandler(IReservaEstoqueRepository repository)
    : IRequestHandler<ReservarPecasCommand, ReservaEstoqueResult>
{
    public async Task<ReservaEstoqueResult> Handle(
        ReservarPecasCommand request,
        CancellationToken cancellationToken)
    {
        var itens = request.Pecas
            .Select(item => (item.PecaId, item.Quantidade))
            .ToArray();
        var reserva = await repository.ReservarAsync(
            request.CorrelationId,
            request.OrdemServicoId,
            request.OrcamentoId,
            itens,
            DateTimeOffset.UtcNow,
            cancellationToken);

        return new ReservaEstoqueResult(
            reserva.EventoResultadoId,
            reserva.CorrelationId,
            reserva.OrdemServicoId,
            reserva.Status,
            reserva.Itens
                .OrderBy(item => item.PecaId)
                .Select(item => new PecaReservaResultado(
                    item.PecaId,
                    item.QuantidadeSolicitada,
                    item.QuantidadeReservada,
                    item.QuantidadeFaltante))
                .ToArray());
    }
}
