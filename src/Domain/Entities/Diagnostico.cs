namespace Fiap.TechChallenge.OficinaMecanica.Execucao.Domain.Entities;

public sealed class Diagnostico
{
    private readonly List<DiagnosticoServicoResultado> _servicosResultado = [];
    private readonly List<DiagnosticoPecaResultado> _pecasResultado = [];

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
    public string? ParecerTecnico { get; private set; }
    public DateTimeOffset? ResultadoAtualizadoEm { get; private set; }
    public int Versao { get; private set; }
    public IReadOnlyCollection<DiagnosticoServicoResultado> ServicosResultado => _servicosResultado;
    public IReadOnlyCollection<DiagnosticoPecaResultado> PecasResultado => _pecasResultado;

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

    public bool RegistrarResultado(
        string parecerTecnico,
        IEnumerable<int> servicoIds,
        IEnumerable<(int PecaId, int Quantidade)> pecas,
        DateTimeOffset atualizadoEm)
    {
        if (!string.Equals(Status, "EmDiagnostico", StringComparison.Ordinal))
            throw new InvalidOperationException("O resultado so pode ser registrado durante o diagnostico.");
        if (string.IsNullOrWhiteSpace(parecerTecnico))
            throw new ArgumentException("O parecer tecnico e obrigatorio.", nameof(parecerTecnico));

        var parecerNormalizado = parecerTecnico.Trim();
        if (parecerNormalizado.Length > 2000)
            throw new ArgumentException("O parecer tecnico deve possuir no maximo 2000 caracteres.", nameof(parecerTecnico));

        var servicosNormalizados = servicoIds.Order().ToArray();
        if (servicosNormalizados.Length == 0 || servicosNormalizados.Any(id => id <= 0))
            throw new ArgumentException("Ao menos um servico diagnosticado valido e obrigatorio.", nameof(servicoIds));
        if (servicosNormalizados.Distinct().Count() != servicosNormalizados.Length)
            throw new ArgumentException("Nao e permitido repetir servicos no resultado.", nameof(servicoIds));

        var pecasNormalizadas = pecas.OrderBy(item => item.PecaId).ToArray();
        if (pecasNormalizadas.Any(item => item.PecaId <= 0 || item.Quantidade <= 0))
            throw new ArgumentException("As pecas devem possuir ID e quantidade validos.", nameof(pecas));
        if (pecasNormalizadas.Select(item => item.PecaId).Distinct().Count() != pecasNormalizadas.Length)
            throw new ArgumentException("Nao e permitido repetir pecas no resultado.", nameof(pecas));

        if (ResultadoCorresponde(parecerNormalizado, servicosNormalizados, pecasNormalizadas))
            return false;

        ParecerTecnico = parecerNormalizado;
        ResultadoAtualizadoEm = new DateTimeOffset(
            atualizadoEm.Ticks - atualizadoEm.Ticks % TimeSpan.TicksPerMicrosecond,
            atualizadoEm.Offset);
        Versao++;

        _servicosResultado.Clear();
        _servicosResultado.AddRange(servicosNormalizados.Select(DiagnosticoServicoResultado.Criar));
        _pecasResultado.Clear();
        _pecasResultado.AddRange(pecasNormalizadas.Select(item =>
            DiagnosticoPecaResultado.Criar(item.PecaId, item.Quantidade)));

        return true;
    }

    private bool ResultadoCorresponde(
        string parecerTecnico,
        IReadOnlyCollection<int> servicoIds,
        IReadOnlyCollection<(int PecaId, int Quantidade)> pecas) =>
        string.Equals(ParecerTecnico, parecerTecnico, StringComparison.Ordinal) &&
        _servicosResultado.Select(item => item.ServicoId).Order().SequenceEqual(servicoIds) &&
        _pecasResultado
            .Select(item => (item.PecaId, item.Quantidade))
            .OrderBy(item => item.PecaId)
            .SequenceEqual(pecas);
}
