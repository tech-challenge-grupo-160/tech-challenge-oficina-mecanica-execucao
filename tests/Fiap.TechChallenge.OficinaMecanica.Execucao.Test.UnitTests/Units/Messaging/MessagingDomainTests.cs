using Fiap.TechChallenge.OficinaMecanica.Execucao.Application.Messaging;
using Fiap.TechChallenge.OficinaMecanica.Execucao.Domain.Entities;
using Fiap.TechChallenge.OficinaMecanica.Execucao.Infrastructure.Messaging;
using FluentAssertions;

namespace Fiap.TechChallenge.OficinaMecanica.Execucao.Test.UnitTests.Units.Messaging;

public class MessagingDomainTests
{
    [Fact]
    public void InboxMessage_CriarDevePreservarMetadadosDoEvento()
    {
        var messageId = Guid.NewGuid();
        var correlationId = Guid.NewGuid();
        var processadoEm = DateTimeOffset.UtcNow;

        var inbox = InboxMessage.Criar(messageId, correlationId, "IniciarDiagnostico", processadoEm);

        inbox.MessageId.Should().Be(messageId);
        inbox.CorrelationId.Should().Be(correlationId);
        inbox.Tipo.Should().Be("IniciarDiagnostico");
        inbox.ProcessadoEm.Should().Be(processadoEm);
    }

    [Fact]
    public void OutboxMessage_DeveRegistrarFalhaEPublicacao()
    {
        var criadoEm = DateTimeOffset.UtcNow;
        var outbox = OutboxMessage.Criar(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "DiagnosticoIniciado",
            "OficinaEventos",
            "{\"status\":\"EmDiagnostico\"}",
            criadoEm);

        outbox.CriadoEm.Should().Be(criadoEm);
        outbox.Tentativas.Should().Be(0);
        outbox.PublicadoEm.Should().BeNull();

        outbox.RegistrarFalha("falha temporaria");

        outbox.Tentativas.Should().Be(1);
        outbox.UltimoErro.Should().Be("falha temporaria");

        var publicadoEm = criadoEm.AddSeconds(1);
        outbox.MarcarPublicada(publicadoEm);

        outbox.PublicadoEm.Should().Be(publicadoEm);
        outbox.UltimoErro.Should().BeNull();
    }

    [Fact]
    public void OutboxMessage_DeveLimitarTamanhoDoUltimoErro()
    {
        var outbox = OutboxMessage.Criar(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "DiagnosticoIniciado",
            "OficinaEventos",
            "{}",
            DateTimeOffset.UtcNow);

        outbox.RegistrarFalha(new string('x', 2100));

        outbox.UltimoErro.Should().HaveLength(2000);
    }

    [Fact]
    public void ContratosDeMensageria_DevemPreservarVersaoECorrelacao()
    {
        var messageId = Guid.NewGuid();
        var correlationId = Guid.NewGuid();
        var ocorridoEm = DateTimeOffset.UtcNow;
        var iniciar = new IniciarDiagnosticoMessage(
            messageId,
            correlationId,
            ocorridoEm,
            3007,
            1004,
            "Ruido durante a frenagem",
            [1000],
            [1000]);
        var iniciado = new DiagnosticoIniciadoMessage(
            Guid.NewGuid(),
            correlationId,
            ocorridoEm,
            3007,
            1,
            "EmDiagnostico",
            ocorridoEm);

        iniciar.MessageId.Should().Be(messageId);
        iniciar.CorrelationId.Should().Be(correlationId);
        iniciar.Versao.Should().Be(1);
        iniciar.ServicoIdsSolicitados.Should().ContainSingle().Which.Should().Be(1000);
        iniciado.CorrelationId.Should().Be(correlationId);
        iniciado.Status.Should().Be("EmDiagnostico");
        iniciado.Versao.Should().Be(1);
    }

    [Fact]
    public void ContratosDeReserva_DevemPreservarVersaoCorrelacaoEItens()
    {
        var correlationId = Guid.NewGuid();
        var orcamentoId = Guid.NewGuid();
        var ocorridoEm = DateTimeOffset.UtcNow;
        IMensagemIntegracao reservar = new ReservarPecasMessage(
            Guid.NewGuid(),
            correlationId,
            ocorridoEm,
            3007,
            orcamentoId,
            [new PecaReservarMessage(1000, 2)]);
        var reservadas = new PecasReservadasMessage(
            Guid.NewGuid(),
            correlationId,
            ocorridoEm,
            3007,
            orcamentoId,
            [new PecaReservaResultadoMessage(1000, 2, 2, 0)]);
        var insuficiente = new EstoqueInsuficienteMessage(
            Guid.NewGuid(),
            correlationId,
            ocorridoEm,
            3008,
            orcamentoId,
            [new PecaReservaResultadoMessage(1000, 4, 0, 4)]);

        reservar.MessageId.Should().NotBeEmpty();
        reservar.CorrelationId.Should().Be(correlationId);
        reservar.Versao.Should().Be(1);
        var reservarMessage = reservar.Should().BeOfType<ReservarPecasMessage>().Subject;
        reservarMessage.OrdemServicoId.Should().Be(3007);
        reservarMessage.OrcamentoId.Should().Be(orcamentoId);
        reservarMessage.OcorridoEm.Should().Be(ocorridoEm);
        reservarMessage.Pecas.Should().ContainSingle()
            .Which.Should().Be(new PecaReservarMessage(1000, 2));

        reservadas.MessageId.Should().NotBeEmpty();
        reservadas.CorrelationId.Should().Be(correlationId);
        reservadas.OrdemServicoId.Should().Be(3007);
        reservadas.OrcamentoId.Should().Be(orcamentoId);
        reservadas.OcorridoEm.Should().Be(ocorridoEm);
        reservadas.Versao.Should().Be(1);
        reservadas.Pecas.Single().QuantidadeReservada.Should().Be(2);
        reservadas.Pecas.Single().QuantidadeFaltante.Should().Be(0);

        insuficiente.MessageId.Should().NotBeEmpty();
        insuficiente.CorrelationId.Should().Be(correlationId);
        insuficiente.OrdemServicoId.Should().Be(3008);
        insuficiente.OrcamentoId.Should().Be(orcamentoId);
        insuficiente.OcorridoEm.Should().Be(ocorridoEm);
        insuficiente.Versao.Should().Be(1);
        insuficiente.Pecas.Single().PecaId.Should().Be(1000);
        insuficiente.Pecas.Single().QuantidadeSolicitada.Should().Be(4);
        insuficiente.Pecas.Single().QuantidadeFaltante.Should().Be(4);
    }

    [Fact]
    public void MessagingOptions_DeveExporFilaDeReservaDePecas()
    {
        var options = new MessagingOptions { ReservaPecasQueueUrl = "http://localhost:4566/000000000000/execucao-reservar-pecas" };

        options.ReservaPecasQueueUrl.Should().EndWith("execucao-reservar-pecas");
        new MessagingOptions().ReservaPecasQueueUrl.Should().BeEmpty();
    }
}
