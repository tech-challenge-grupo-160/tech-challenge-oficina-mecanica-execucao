using Fiap.TechChallenge.OficinaMecanica.Execucao.Domain.Entities;

namespace Fiap.TechChallenge.OficinaMecanica.Execucao.Application.Diagnosticos;

public sealed record ConclusaoDiagnosticoResult(
    int DiagnosticoId,
    int OrdemServicoId,
    string Status,
    string ParecerTecnico,
    IReadOnlyCollection<ServicoConclusaoDiagnosticoResult> Servicos,
    IReadOnlyCollection<PecaConclusaoDiagnosticoResult> Pecas,
    DateTimeOffset ConcluidoEm,
    bool Alterado)
{
    public static ConclusaoDiagnosticoResult Criar(
        Diagnostico diagnostico,
        IReadOnlyDictionary<int, Servico> servicos,
        IReadOnlyDictionary<int, Peca> pecas,
        bool alterado)
    {
        if (diagnostico.ParecerTecnico is null || diagnostico.ConcluidoEm is null)
            throw new InvalidOperationException("O diagnostico ainda nao foi concluido.");

        return new ConclusaoDiagnosticoResult(
            diagnostico.Id,
            diagnostico.OrdemServicoId,
            diagnostico.Status,
            diagnostico.ParecerTecnico,
            diagnostico.ServicosResultado
                .OrderBy(item => item.ServicoId)
                .Select(item =>
                {
                    var servico = servicos[item.ServicoId];
                    return new ServicoConclusaoDiagnosticoResult(
                        servico.Id,
                        servico.Nome,
                        servico.Preco,
                        servico.TempoEstimado);
                })
                .ToArray(),
            diagnostico.PecasResultado
                .OrderBy(item => item.PecaId)
                .Select(item =>
                {
                    var peca = pecas[item.PecaId];
                    return new PecaConclusaoDiagnosticoResult(
                        peca.Id,
                        peca.Nome,
                        item.Quantidade,
                        peca.Preco);
                })
                .ToArray(),
            diagnostico.ConcluidoEm.Value,
            alterado);
    }
}

public sealed record ServicoConclusaoDiagnosticoResult(
    int ServicoId,
    string Nome,
    decimal Preco,
    int TempoEstimado);

public sealed record PecaConclusaoDiagnosticoResult(
    int PecaId,
    string Nome,
    int Quantidade,
    decimal Preco);
