using System.Data;
using System.Text.Json;
using Fiap.TechChallenge.OficinaMecanica.Execucao.Application.Abstractions;
using Fiap.TechChallenge.OficinaMecanica.Execucao.Application.Exceptions;
using Fiap.TechChallenge.OficinaMecanica.Execucao.Application.Messaging;
using Fiap.TechChallenge.OficinaMecanica.Execucao.Domain.Entities;
using Fiap.TechChallenge.OficinaMecanica.Execucao.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Fiap.TechChallenge.OficinaMecanica.Execucao.Infrastructure.Repositories;

public sealed class ReservaEstoqueRepository(ExecucaoDbContext context)
    : IReservaEstoqueRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<ReservaEstoque> ReservarAsync(
        Guid correlationId,
        int ordemServicoId,
        Guid orcamentoId,
        IReadOnlyCollection<(int PecaId, int Quantidade)> itens,
        DateTimeOffset criadaEm,
        CancellationToken cancellationToken)
    {
        var existente = await context.ReservasEstoque
            .Include(reserva => reserva.Itens)
            .SingleOrDefaultAsync(
                reserva => reserva.OrdemServicoId == ordemServicoId,
                cancellationToken);
        if (existente is not null)
        {
            if (!existente.Corresponde(correlationId, orcamentoId, itens))
                throw new InvalidOperationException(
                    "A ordem de servico ja possui uma reserva de estoque incompatível.");
            return existente;
        }

        var ids = itens.Select(item => item.PecaId).Distinct().ToArray();
        var pecas = await context.Pecas
            .Where(peca => peca.Ativa && ids.Contains(peca.Id))
            .ToDictionaryAsync(peca => peca.Id, cancellationToken);
        var resultados = itens.Select(item =>
        {
            var reservada = pecas.TryGetValue(item.PecaId, out var peca)
                ? peca.ReservarSeDisponivel(item.Quantidade)
                : 0;
            return ReservaEstoqueItem.Criar(item.PecaId, item.Quantidade, reservada);
        }).ToArray();

        var reserva = ReservaEstoque.Criar(
            correlationId,
            ordemServicoId,
            orcamentoId,
            resultados,
            criadaEm);
        context.ReservasEstoque.Add(reserva);
        return reserva;
    }

    public async Task<ReposicaoEstoqueProcessada> ReporAsync(
        int pecaId,
        int quantidade,
        DateTimeOffset repostaEm,
        CancellationToken cancellationToken)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(
            IsolationLevel.Serializable, cancellationToken);
        var peca = await context.Pecas.SingleOrDefaultAsync(
            item => item.Id == pecaId && item.Ativa,
            cancellationToken)
            ?? throw new ServiceNotFoundException($"Peca ativa com ID {pecaId} nao encontrada.");

        peca.Repor(quantidade);
        var pendentes = await context.ReservasEstoque
            .Include(reserva => reserva.Itens)
            .Where(reserva =>
                reserva.Status == "EstoqueInsuficiente" &&
                reserva.Itens.Any(item => item.PecaId == pecaId &&
                                          item.QuantidadeReservada < item.QuantidadeSolicitada))
            .OrderBy(reserva => reserva.CriadaEm)
            .ThenBy(reserva => reserva.Id)
            .ToListAsync(cancellationToken);

        var ordensLiberadas = new List<int>();
        foreach (var reserva in pendentes)
        {
            var item = reserva.Itens.Single(item => item.PecaId == pecaId);
            if (peca.QuantidadeEstoque < item.QuantidadeFaltante)
                break;

            if (!reserva.TentarCompletarItem(pecaId, peca) || reserva.Status != "Reservada")
                continue;

            AdicionarEventoReservaConcluida(reserva, repostaEm);
            ordensLiberadas.Add(reserva.OrdemServicoId);
        }

        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return new ReposicaoEstoqueProcessada(
            peca.Id,
            quantidade,
            peca.QuantidadeEstoque,
            ordensLiberadas);
    }

    private void AdicionarEventoReservaConcluida(
        ReservaEstoque reserva,
        DateTimeOffset ocorridoEm)
    {
        var evento = new PecasReservadasMessage(
            reserva.EventoResultadoId,
            reserva.CorrelationId,
            ocorridoEm,
            reserva.OrdemServicoId,
            reserva.OrcamentoId,
            reserva.Itens
                .OrderBy(item => item.PecaId)
                .Select(item => new PecaReservaResultadoMessage(
                    item.PecaId,
                    item.QuantidadeSolicitada,
                    item.QuantidadeReservada,
                    item.QuantidadeFaltante))
                .ToArray());
        context.OutboxMessages.Add(OutboxMessage.Criar(
            evento.MessageId,
            evento.CorrelationId,
            nameof(PecasReservadasMessage),
            "OficinaEventos",
            JsonSerializer.Serialize(evento, JsonOptions),
            ocorridoEm));
    }
}
