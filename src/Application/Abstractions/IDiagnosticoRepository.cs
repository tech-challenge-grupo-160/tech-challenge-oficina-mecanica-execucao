using Fiap.TechChallenge.OficinaMecanica.Execucao.Domain.Entities;

namespace Fiap.TechChallenge.OficinaMecanica.Execucao.Application.Abstractions;

public interface IDiagnosticoRepository
{
    Task<(Diagnostico Diagnostico, bool Criado)> CriarOuObterAsync(
        Diagnostico diagnostico,
        CancellationToken cancellationToken);

    Task<Diagnostico?> ObterPorOrdemServicoIdAsync(
        int ordemServicoId,
        CancellationToken cancellationToken);

    Task<Diagnostico?> ObterParaAtualizacaoPorOrdemServicoIdAsync(
        int ordemServicoId,
        CancellationToken cancellationToken);

    Task AtualizarAsync(
        Diagnostico diagnostico,
        CancellationToken cancellationToken);
}
