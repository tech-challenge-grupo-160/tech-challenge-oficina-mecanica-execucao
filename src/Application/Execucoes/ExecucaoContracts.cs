using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Execucao.Application.Execucoes;

public sealed record IniciarExecucaoCommand(
    Guid CorrelationId,
    int OrdemServicoId,
    int DiagnosticoId,
    Guid OrcamentoId,
    DateTimeOffset IniciadaEm) : IRequest<ExecucaoResult>;

public sealed record ObterExecucaoPorOrdemQuery(int OrdemServicoId) : IRequest<ExecucaoResult>;

public sealed record ListarFilaExecucaoQuery : IRequest<IReadOnlyCollection<ExecucaoResult>>;

public sealed record FinalizarExecucaoCommand(int OrdemServicoId) : IRequest<ExecucaoResult>;

public sealed record ExecucaoResult(
    Guid Id,
    Guid CorrelationId,
    int OrdemServicoId,
    int DiagnosticoId,
    Guid OrcamentoId,
    string Status,
    DateTimeOffset IniciadaEm,
    DateTimeOffset? FinalizadaEm);
