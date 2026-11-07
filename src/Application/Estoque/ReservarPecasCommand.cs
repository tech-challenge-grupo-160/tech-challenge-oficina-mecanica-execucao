using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Execucao.Application.Estoque;

public sealed record ReservarPecasCommand(
    Guid CorrelationId,
    int OrdemServicoId,
    IReadOnlyCollection<PecaReservaCommand> Pecas)
    : IRequest<ReservaEstoqueResult>;

public sealed record PecaReservaCommand(int PecaId, int Quantidade);

public sealed record ReservaEstoqueResult(
    Guid EventoResultadoId,
    Guid CorrelationId,
    int OrdemServicoId,
    string Status,
    IReadOnlyCollection<PecaReservaResultado> Pecas);

public sealed record PecaReservaResultado(
    int PecaId,
    int QuantidadeSolicitada,
    int QuantidadeReservada,
    int QuantidadeFaltante);
