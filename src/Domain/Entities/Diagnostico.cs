namespace Fiap.TechChallenge.OficinaMecanica.Execucao.Domain.Entities;

public sealed class Diagnostico
{
    private Diagnostico() { }

    public int Id { get; private set; }
    public int OrdemServicoId { get; private set; }
    public int VeiculoId { get; private set; }
    public string DescricaoSolicitacao { get; private set; } = null!;
    public int[] ServicoIdsSolicitados { get; private set; } = [];
    public int[] PecaIdsSolicitadas { get; private set; } = [];
    public string ChaveIdempotencia { get; private set; } = null!;
    public string Status { get; private set; } = null!;
    public DateTimeOffset IniciadoEm { get; private set; }

    public static Diagnostico Iniciar(
        int ordemServicoId,
        int veiculoId,
        string descricaoSolicitacao,
        IEnumerable<int> servicoIdsSolicitados,
        IEnumerable<int> pecaIdsSolicitadas,
        string chaveIdempotencia,
        DateTimeOffset iniciadoEm)
    {
        if (ordemServicoId <= 0)
            throw new ArgumentException("OrdemServicoId deve ser maior que zero.", nameof(ordemServicoId));
        if (veiculoId <= 0)
            throw new ArgumentException("VeiculoId deve ser maior que zero.", nameof(veiculoId));
        if (string.IsNullOrWhiteSpace(descricaoSolicitacao))
            throw new ArgumentException("Descricao da solicitacao e obrigatoria.", nameof(descricaoSolicitacao));
        if (string.IsNullOrWhiteSpace(chaveIdempotencia))
            throw new ArgumentException("Chave de idempotencia e obrigatoria.", nameof(chaveIdempotencia));

        var servicoIds = servicoIdsSolicitados.Distinct().Order().ToArray();
        if (servicoIds.Length == 0 || servicoIds.Any(id => id <= 0))
            throw new ArgumentException("Ao menos um servico solicitado valido e obrigatorio.", nameof(servicoIdsSolicitados));
        var pecaIds = pecaIdsSolicitadas.Distinct().Order().ToArray();
        if (pecaIds.Any(id => id <= 0))
            throw new ArgumentException("As pecas solicitadas devem possuir IDs validos.", nameof(pecaIdsSolicitadas));

        return new Diagnostico
        {
            OrdemServicoId = ordemServicoId,
            VeiculoId = veiculoId,
            DescricaoSolicitacao = descricaoSolicitacao.Trim(),
            ServicoIdsSolicitados = servicoIds,
            PecaIdsSolicitadas = pecaIds,
            ChaveIdempotencia = chaveIdempotencia.Trim(),
            Status = "EmDiagnostico",
            IniciadoEm = iniciadoEm
        };
    }

    public bool CorrespondeA(Diagnostico outro) =>
        OrdemServicoId == outro.OrdemServicoId &&
        VeiculoId == outro.VeiculoId &&
        string.Equals(DescricaoSolicitacao, outro.DescricaoSolicitacao, StringComparison.Ordinal) &&
        ServicoIdsSolicitados.SequenceEqual(outro.ServicoIdsSolicitados) &&
        PecaIdsSolicitadas.SequenceEqual(outro.PecaIdsSolicitadas) &&
        string.Equals(ChaveIdempotencia, outro.ChaveIdempotencia, StringComparison.Ordinal);
}
