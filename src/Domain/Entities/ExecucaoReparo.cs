namespace Fiap.TechChallenge.OficinaMecanica.Execucao.Domain.Entities;

public sealed class ExecucaoReparo
{
    private ExecucaoReparo() { }

    public Guid Id { get; private set; }
    public Guid CorrelationId { get; private set; }
    public int OrdemServicoId { get; private set; }
    public int DiagnosticoId { get; private set; }
    public Guid OrcamentoId { get; private set; }
    public string Status { get; private set; } = null!;
    public DateTimeOffset IniciadaEm { get; private set; }
    public DateTimeOffset? FinalizadaEm { get; private set; }
    public Guid? EventoFinalizacaoId { get; private set; }

    public static ExecucaoReparo Criar(
        Guid correlationId,
        int ordemServicoId,
        int diagnosticoId,
        Guid orcamentoId,
        DateTimeOffset iniciadaEm)
    {
        if (correlationId == Guid.Empty)
            throw new ArgumentException("CorrelationId e obrigatorio.", nameof(correlationId));
        if (ordemServicoId <= 0)
            throw new ArgumentException("OrdemServicoId deve ser maior que zero.", nameof(ordemServicoId));
        if (diagnosticoId <= 0)
            throw new ArgumentException("DiagnosticoId deve ser maior que zero.", nameof(diagnosticoId));
        if (orcamentoId == Guid.Empty)
            throw new ArgumentException("OrcamentoId e obrigatorio.", nameof(orcamentoId));

        return new ExecucaoReparo
        {
            Id = Guid.NewGuid(),
            CorrelationId = correlationId,
            OrdemServicoId = ordemServicoId,
            DiagnosticoId = diagnosticoId,
            OrcamentoId = orcamentoId,
            Status = "EmExecucao",
            IniciadaEm = iniciadaEm
        };
    }

    public bool Corresponde(Guid correlationId, int diagnosticoId, Guid orcamentoId) =>
        CorrelationId == correlationId &&
        DiagnosticoId == diagnosticoId &&
        OrcamentoId == orcamentoId;

    public bool Finalizar(DateTimeOffset finalizadaEm)
    {
        if (Status == "Finalizada")
            return false;
        if (Status != "EmExecucao")
            throw new InvalidOperationException($"Execucao no status {Status} nao pode ser finalizada.");

        Status = "Finalizada";
        FinalizadaEm = finalizadaEm;
        EventoFinalizacaoId = Guid.NewGuid();
        return true;
    }
}
