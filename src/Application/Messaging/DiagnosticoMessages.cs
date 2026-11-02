namespace Fiap.TechChallenge.OficinaMecanica.Execucao.Application.Messaging;

public sealed record IniciarDiagnosticoMessage(
    Guid MessageId,
    Guid CorrelationId,
    DateTimeOffset OcorridoEm,
    int OrdemServicoId,
    int VeiculoId,
    string DescricaoSolicitacao,
    IReadOnlyCollection<int> ServicoIdsSolicitados,
    IReadOnlyCollection<int> PecaIdsSolicitadas,
    int Versao = 1);

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
