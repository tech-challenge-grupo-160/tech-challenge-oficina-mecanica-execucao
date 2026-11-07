using System.Diagnostics.CodeAnalysis;
using Amazon.SQS;
using Fiap.TechChallenge.OficinaMecanica.Execucao.Application.Execucoes;
using Fiap.TechChallenge.OficinaMecanica.Execucao.Application.Messaging;
using Fiap.TechChallenge.OficinaMecanica.Execucao.Infrastructure.Data;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Fiap.TechChallenge.OficinaMecanica.Execucao.Infrastructure.Messaging;

[ExcludeFromCodeCoverage]
public sealed class InicioExecucaoConsumerWorker(
    IServiceScopeFactory scopeFactory,
    IAmazonSQS sqs,
    IOptions<MessagingOptions> options,
    ILogger<InicioExecucaoConsumerWorker> logger)
    : SqsConsumerWorker<IniciarExecucaoMessage>(scopeFactory, sqs, logger)
{
    protected override bool Enabled => options.Value.Enabled;
    protected override string QueueUrl => options.Value.InicioExecucaoQueueUrl;

    protected override async Task ProcessarComandoAsync(
        IniciarExecucaoMessage comando,
        IServiceProvider serviceProvider,
        ExecucaoDbContext context,
        CancellationToken cancellationToken)
    {
        var mediator = serviceProvider.GetRequiredService<ISender>();
        await mediator.Send(new IniciarExecucaoCommand(
            comando.CorrelationId,
            comando.OrdemServicoId,
            comando.DiagnosticoId,
            comando.OrcamentoId,
            comando.OcorridoEm), cancellationToken);
    }
}
