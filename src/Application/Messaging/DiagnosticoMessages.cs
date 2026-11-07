namespace Fiap.TechChallenge.OficinaMecanica.Execucao.Application.Messaging;

public interface IMensagemIntegracao
{
    Guid MessageId { get; }
    Guid CorrelationId { get; }
    int Versao { get; }
}

public sealed record IniciarDiagnosticoMessage(
    Guid MessageId,
    Guid CorrelationId,
    DateTimeOffset OcorridoEm,
    int OrdemServicoId,
    int VeiculoId,
    string DescricaoSolicitacao,
    IReadOnlyCollection<int> ServicoIdsSolicitados,
    IReadOnlyCollection<int> PecaIdsSolicitadas,
    int Versao = 1) : IMensagemIntegracao;

public sealed record DiagnosticoIniciadoMessage(
    Guid MessageId,
    Guid CorrelationId,
    DateTimeOffset OcorridoEm,
    int OrdemServicoId,
    int DiagnosticoId,
    string Status,
    DateTimeOffset IniciadoEm,
    int Versao = 1);

public sealed record DiagnosticoConcluidoMessage(
    Guid MessageId,
    Guid CorrelationId,
    DateTimeOffset OcorridoEm,
    int OrdemServicoId,
    int DiagnosticoId,
    string Status,
    string ParecerTecnico,
    IReadOnlyCollection<ServicoDiagnosticoConcluidoMessage> Servicos,
    IReadOnlyCollection<PecaDiagnosticoConcluidoMessage> Pecas,
    DateTimeOffset ConcluidoEm,
    int Versao = 1);

public sealed record ServicoDiagnosticoConcluidoMessage(
    int ServicoId,
    string Nome,
    decimal Preco,
    int TempoEstimado);

public sealed record PecaDiagnosticoConcluidoMessage(
    int PecaId,
    string Nome,
    int Quantidade,
    decimal Preco);

public sealed record ReservarPecasMessage(
    Guid MessageId,
    Guid CorrelationId,
    DateTimeOffset OcorridoEm,
    int OrdemServicoId,
    Guid OrcamentoId,
    IReadOnlyCollection<PecaReservarMessage> Pecas,
    int Versao = 1) : IMensagemIntegracao;

public sealed record PecaReservarMessage(int PecaId, int Quantidade);

public sealed record PecasReservadasMessage(
    Guid MessageId,
    Guid CorrelationId,
    DateTimeOffset OcorridoEm,
    int OrdemServicoId,
    Guid OrcamentoId,
    IReadOnlyCollection<PecaReservaResultadoMessage> Pecas,
    int Versao = 1);

public sealed record EstoqueInsuficienteMessage(
    Guid MessageId,
    Guid CorrelationId,
    DateTimeOffset OcorridoEm,
    int OrdemServicoId,
    Guid OrcamentoId,
    IReadOnlyCollection<PecaReservaResultadoMessage> Pecas,
    int Versao = 1);

public sealed record PecaReservaResultadoMessage(
    int PecaId,
    int QuantidadeSolicitada,
    int QuantidadeReservada,
    int QuantidadeFaltante);

public sealed record IniciarExecucaoMessage(
    Guid MessageId,
    Guid CorrelationId,
    DateTimeOffset OcorridoEm,
    int OrdemServicoId,
    int DiagnosticoId,
    Guid OrcamentoId,
    int Versao = 1) : IMensagemIntegracao;

public sealed record ExecucaoFinalizadaMessage(
    Guid MessageId,
    Guid CorrelationId,
    DateTimeOffset OcorridoEm,
    int OrdemServicoId,
    int DiagnosticoId,
    Guid OrcamentoId,
    string Status,
    DateTimeOffset FinalizadaEm,
    int Versao = 1);
