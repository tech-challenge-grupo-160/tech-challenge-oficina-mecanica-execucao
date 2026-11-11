using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Fiap.TechChallenge.OficinaMecanica.Execucao.Infrastructure.Data;
using Fiap.TechChallenge.OficinaMecanica.Execucao.Infrastructure.Logging;
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
            using var contexto = logger.Iniciar(
                mensagem.CorrelationId,
                OutboxLogContexto.LerOrdemServicoId(mensagem.Payload),
                mensagem.Id,
                mensagem.Tipo);
            try
            {
                await sns.PublishAsync(new PublishRequest
                {
                    TopicArn = options.Value.EventosTopicArn,
                    Message = mensagem.Payload,
                    MessageAttributes = new Dictionary<string, MessageAttributeValue>
                    {
                        ["tipo"] = new() { DataType = "String", StringValue = mensagem.Tipo }
                    }
                }, cancellationToken);
                mensagem.MarcarPublicada(DateTimeOffset.UtcNow);
                logger.LogInformation("Evento {TipoMensagem} publicado.", mensagem.Tipo);
            }
            catch (Exception exception)
            {
                mensagem.RegistrarFalha(exception.Message);
                logger.LogWarning(exception, "Falha ao publicar evento {MessageId}.", mensagem.Id);
            }
        }

        if (mensagens.Count > 0)
            await context.SaveChangesAsync(cancellationToken);
    }
}
