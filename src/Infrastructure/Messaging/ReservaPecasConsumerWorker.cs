using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Amazon.SQS;
using Fiap.TechChallenge.OficinaMecanica.Execucao.Application.Estoque;
using Fiap.TechChallenge.OficinaMecanica.Execucao.Application.Messaging;
using Fiap.TechChallenge.OficinaMecanica.Execucao.Domain.Entities;
using Fiap.TechChallenge.OficinaMecanica.Execucao.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Fiap.TechChallenge.OficinaMecanica.Execucao.Infrastructure.Messaging;

[ExcludeFromCodeCoverage]
public sealed class ReservaPecasConsumerWorker(
    IServiceScopeFactory scopeFactory,
    IAmazonSQS sqs,
    IOptions<MessagingOptions> options,
    ILogger<ReservaPecasConsumerWorker> logger)
    : SqsConsumerWorker<ReservarPecasMessage>(scopeFactory, sqs, logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    protected override bool Enabled => options.Value.Enabled;
    protected override string QueueUrl => options.Value.ReservaPecasQueueUrl;

    protected override async Task ProcessarComandoAsync(
        ReservarPecasMessage comando,
        IServiceProvider serviceProvider,
        ExecucaoDbContext context,
        CancellationToken cancellationToken)
    {
        var mediator = serviceProvider.GetRequiredService<ISender>();
        var resultado = await mediator.Send(new ReservarPecasCommand(
            comando.CorrelationId,
            comando.OrdemServicoId,
            comando.Pecas.Select(item =>
                new PecaReservaCommand(item.PecaId, item.Quantidade)).ToArray()), cancellationToken);

        if (await context.OutboxMessages.AnyAsync(
                item => item.Id == resultado.EventoResultadoId,
                cancellationToken))
            return;

        object evento = resultado.Status == "Reservada"
            ? new PecasReservadasMessage(
                resultado.EventoResultadoId,
                resultado.CorrelationId,
                DateTimeOffset.UtcNow,
                resultado.OrdemServicoId,
                comando.OrcamentoId,
                MapearItens(resultado))
            : new EstoqueInsuficienteMessage(
                resultado.EventoResultadoId,
                resultado.CorrelationId,
                DateTimeOffset.UtcNow,
                resultado.OrdemServicoId,
                comando.OrcamentoId,
                MapearItens(resultado));
        context.OutboxMessages.Add(OutboxMessage.Criar(
            resultado.EventoResultadoId,
            resultado.CorrelationId,
            evento.GetType().Name,
            "OficinaEventos",
            JsonSerializer.Serialize(evento, evento.GetType(), JsonOptions),
            DateTimeOffset.UtcNow));
    }

    private static PecaReservaResultadoMessage[] MapearItens(ReservaEstoqueResult resultado) =>
        resultado.Pecas.Select(item => new PecaReservaResultadoMessage(
            item.PecaId,
            item.QuantidadeSolicitada,
            item.QuantidadeReservada,
            item.QuantidadeFaltante)).ToArray();
}
