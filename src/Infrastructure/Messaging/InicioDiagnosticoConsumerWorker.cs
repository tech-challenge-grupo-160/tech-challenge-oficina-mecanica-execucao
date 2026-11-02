using System.Text.Json;
using Amazon.SQS;
using Amazon.SQS.Model;
using Fiap.TechChallenge.OficinaMecanica.Execucao.Application.Diagnosticos;
using Fiap.TechChallenge.OficinaMecanica.Execucao.Application.Messaging;
using Fiap.TechChallenge.OficinaMecanica.Execucao.Domain.Entities;
using Fiap.TechChallenge.OficinaMecanica.Execucao.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Fiap.TechChallenge.OficinaMecanica.Execucao.Infrastructure.Messaging;

public sealed class InicioDiagnosticoConsumerWorker(
    IServiceScopeFactory scopeFactory,
    IAmazonSQS sqs,
    IOptions<MessagingOptions> options,
    ILogger<InicioDiagnosticoConsumerWorker> logger) : BackgroundService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!options.Value.Enabled)
            return;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var response = await sqs.ReceiveMessageAsync(new ReceiveMessageRequest
                {
                    QueueUrl = options.Value.InicioDiagnosticoQueueUrl,
                    MaxNumberOfMessages = 10,
                    WaitTimeSeconds = 10,
                    VisibilityTimeout = 30
                }, stoppingToken);

                foreach (var message in response.Messages ?? [])
                    await ProcessarAsync(message, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Falha ao consumir comandos de inicio do diagnostico.");
                await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
            }
        }
    }

    private async Task ProcessarAsync(Message message, CancellationToken cancellationToken)
    {
        try
        {
            var comando = JsonSerializer.Deserialize<IniciarDiagnosticoMessage>(message.Body, JsonOptions)
                ?? throw new InvalidOperationException("Comando IniciarDiagnostico invalido.");
            if (comando.Versao != 1)
                throw new InvalidOperationException($"Versao de comando nao suportada: {comando.Versao}.");

            await using var scope = scopeFactory.CreateAsyncScope();
            var context = scope.ServiceProvider.GetRequiredService<ExecucaoDbContext>();
            await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

            if (await context.InboxMessages.AnyAsync(
                    item => item.MessageId == comando.MessageId,
                    cancellationToken))
            {
                await transaction.CommitAsync(cancellationToken);
                await ExcluirAsync(message, cancellationToken);
                return;
            }

            context.InboxMessages.Add(InboxMessage.Criar(
                comando.MessageId,
                comando.CorrelationId,
                nameof(IniciarDiagnosticoMessage),
                DateTimeOffset.UtcNow));

            var mediator = scope.ServiceProvider.GetRequiredService<ISender>();
            var resultado = await mediator.Send(new IniciarDiagnosticoCommand(
                comando.OrdemServicoId,
                comando.VeiculoId,
                comando.DescricaoSolicitacao,
                comando.ServicoIdsSolicitados,
                comando.PecaIdsSolicitadas,
                $"iniciar-diagnostico:{comando.OrdemServicoId}"), cancellationToken);

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

            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            await ExcluirAsync(message, cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Falha ao processar mensagem SQS {SqsMessageId}.", message.MessageId);
        }
    }

    private Task<DeleteMessageResponse> ExcluirAsync(
        Message message,
        CancellationToken cancellationToken) =>
        sqs.DeleteMessageAsync(
            options.Value.InicioDiagnosticoQueueUrl,
            message.ReceiptHandle,
            cancellationToken);
}
