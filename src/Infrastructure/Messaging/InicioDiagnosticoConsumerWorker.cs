using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Amazon.SQS;
using Fiap.TechChallenge.OficinaMecanica.Execucao.Application.Diagnosticos;
using Fiap.TechChallenge.OficinaMecanica.Execucao.Application.Messaging;
using Fiap.TechChallenge.OficinaMecanica.Execucao.Domain.Entities;
using Fiap.TechChallenge.OficinaMecanica.Execucao.Infrastructure.Data;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Fiap.TechChallenge.OficinaMecanica.Execucao.Infrastructure.Messaging;

[ExcludeFromCodeCoverage]
public sealed class InicioDiagnosticoConsumerWorker(
    IServiceScopeFactory scopeFactory,
    IAmazonSQS sqs,
    IOptions<MessagingOptions> options,
    ILogger<InicioDiagnosticoConsumerWorker> logger)
    : SqsConsumerWorker<IniciarDiagnosticoMessage>(scopeFactory, sqs, logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    protected override bool Enabled => options.Value.Enabled;
    protected override string QueueUrl => options.Value.InicioDiagnosticoQueueUrl;

    protected override async Task ProcessarComandoAsync(
        IniciarDiagnosticoMessage comando,
        IServiceProvider serviceProvider,
        ExecucaoDbContext context,
        CancellationToken cancellationToken)
    {
        var mediator = serviceProvider.GetRequiredService<ISender>();
        var resultado = await mediator.Send(new IniciarDiagnosticoCommand(
            comando.OrdemServicoId,
            comando.VeiculoId,
            comando.DescricaoSolicitacao,
            comando.ServicoIdsSolicitados,
            comando.PecaIdsSolicitadas,
            $"iniciar-diagnostico:{comando.OrdemServicoId}",
            comando.CorrelationId), cancellationToken);

        var evento = new DiagnosticoIniciadoMessage(
            Guid.NewGuid(),
            comando.CorrelationId,
            DateTimeOffset.UtcNow,
            resultado.OrdemServicoId,
            resultado.Id,
            resultado.Status,
            resultado.IniciadoEm);
        context.OutboxMessages.Add(OutboxMessage.Criar(
            evento.MessageId,
            evento.CorrelationId,
            nameof(DiagnosticoIniciadoMessage),
            "OficinaEventos",
            JsonSerializer.Serialize(evento, JsonOptions),
            evento.OcorridoEm));
    }
}
