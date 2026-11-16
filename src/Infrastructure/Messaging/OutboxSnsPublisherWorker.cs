using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Fiap.TechChallenge.OficinaMecanica.Execucao.Infrastructure.Data;
using Fiap.TechChallenge.OficinaMecanica.Execucao.Infrastructure.Logging;
using Fiap.TechChallenge.OficinaMecanica.Execucao.Infrastructure.Observability;
using Fiap.TechChallenge.OficinaMecanica.Execucao.Shared.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Fiap.TechChallenge.OficinaMecanica.Execucao.Infrastructure.Messaging;

public sealed class OutboxSnsPublisherWorker(
    IServiceScopeFactory scopeFactory,
    IAmazonSimpleNotificationService sns,
    IOptions<MessagingOptions> options,
    ILogger<OutboxSnsPublisherWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!options.Value.Enabled)
            return;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PublicarPendentesAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Falha ao publicar eventos da Outbox no SNS.");
            }

            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
        }
    }

    private async Task PublicarPendentesAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<ExecucaoDbContext>();
        var mensagens = await context.OutboxMessages
            .Where(item => item.PublicadoEm == null && item.Destino == "OficinaEventos")
            .OrderBy(item => item.CriadoEm)
            .Take(10)
            .ToListAsync(cancellationToken);

        foreach (var mensagem in mensagens)
        {
            var ordemServicoId = OutboxLogContexto.LerOrdemServicoId(mensagem.Payload);
            // A publicacao continua o trace de quem gravou a mensagem; se foi uma
            // requisicao HTTP (etapa humana), o trace da Saga (F4-31).
            var contextoDaSaga = await context.InboxMessages
                .AsNoTracking()
                .Where(item => item.CorrelationId == mensagem.CorrelationId && item.TraceContext != null)
                .OrderByDescending(item => item.ProcessadoEm)
                .Select(item => item.TraceContext)
                .FirstOrDefaultAsync(cancellationToken);
            using var span = MensageriaTrace.IniciarPublicacao(
                MensageriaTrace.EscolherContextoPai(mensagem.TraceContext, contextoDaSaga),
                mensagem.Tipo,
                MensageriaTrace.SistemaSns,
                mensagem.Destino,
                mensagem.CorrelationId,
                ordemServicoId);
            using var contexto = logger.Iniciar(
                mensagem.CorrelationId,
                ordemServicoId,
                mensagem.Id,
                mensagem.Tipo);
            try
            {
                var atributos = new Dictionary<string, MessageAttributeValue>
                {
                    ["tipo"] = new() { DataType = "String", StringValue = mensagem.Tipo }
                };
                if (MensageriaTrace.CapturarContextoAtual() is { } traceContext)
                    atributos[MensageriaTrace.AtributoMensagem] = new() { DataType = "String", StringValue = traceContext };

                await sns.PublishAsync(new PublishRequest
                {
                    TopicArn = options.Value.EventosTopicArn,
                    Message = mensagem.Payload,
                    MessageAttributes = atributos
                }, cancellationToken);
                mensagem.MarcarPublicada(DateTimeOffset.UtcNow);
                logger.LogInformation("Evento {TipoMensagem} publicado.", mensagem.Tipo);
            }
            catch (Exception exception)
            {
                span.Span.SetException(exception);
                mensagem.RegistrarFalha(exception.Message);
                logger.LogWarning(exception, "Falha ao publicar evento {MessageId}.", mensagem.Id);
            }
        }

        if (mensagens.Count > 0)
            await context.SaveChangesAsync(cancellationToken);
    }
}
