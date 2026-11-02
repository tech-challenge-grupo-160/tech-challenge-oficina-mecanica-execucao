using Fiap.TechChallenge.OficinaMecanica.Execucao.Domain.Entities;

namespace Fiap.TechChallenge.OficinaMecanica.Execucao.Application.Diagnosticos;

public sealed record ResultadoDiagnosticoResult(
    int DiagnosticoId,
    int OrdemServicoId,
    string ParecerTecnico,
    IReadOnlyCollection<int> ServicoIds,
    IReadOnlyCollection<PecaResultadoDiagnosticoResult> Pecas,
    DateTimeOffset AtualizadoEm,
    int Versao)
{
    public static ResultadoDiagnosticoResult Criar(Diagnostico diagnostico)
    {
        if (diagnostico.ParecerTecnico is null || diagnostico.ResultadoAtualizadoEm is null)
            throw new InvalidOperationException("O diagnostico ainda nao possui resultado tecnico.");

        return new ResultadoDiagnosticoResult(
            diagnostico.Id,
            diagnostico.OrdemServicoId,
            diagnostico.ParecerTecnico,
            diagnostico.ServicosResultado.Select(item => item.ServicoId).Order().ToArray(),
            diagnostico.PecasResultado
                .OrderBy(item => item.PecaId)
                .Select(item => new PecaResultadoDiagnosticoResult(item.PecaId, item.Quantidade))
                .ToArray(),
            diagnostico.ResultadoAtualizadoEm.Value,
            diagnostico.Versao);
    }
}

public sealed record PecaResultadoDiagnosticoResult(int PecaId, int Quantidade);
