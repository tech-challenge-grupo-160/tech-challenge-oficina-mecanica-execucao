using System.Net;
using System.Text.Json;
using Fiap.TechChallenge.OficinaMecanica.Execucao.Domain.Entities;
using Fiap.TechChallenge.OficinaMecanica.Execucao.Application.Abstractions;
using Fiap.TechChallenge.OficinaMecanica.Execucao.Infrastructure.Data;
using Fiap.TechChallenge.OficinaMecanica.Execucao.Test.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Fiap.TechChallenge.OficinaMecanica.Execucao.Test.IntegrationTests.Endpoints.Execucoes;

public sealed class ExecucoesControllerTests(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>
{
    [Fact]
    public async Task Fluxo_DeveListarConsultarEFinalizarReparoIdempotentemente()
    {
        const int ordemServicoId = 3901;
        await CriarExecucaoAsync(ordemServicoId);
        using var client = factory.CreateClient();

        var fila = await client.GetAsync("/api/v1/execucoes/fila");
        var consulta = await client.GetAsync($"/api/v1/execucoes/ordem-servico/{ordemServicoId}");
        var primeira = await client.PatchAsync(
            $"/api/v1/execucoes/ordem-servico/{ordemServicoId}/finalizar", null);
        var repetida = await client.PatchAsync(
            $"/api/v1/execucoes/ordem-servico/{ordemServicoId}/finalizar", null);

        fila.StatusCode.Should().Be(HttpStatusCode.OK);
        (await fila.Content.ReadAsStringAsync()).Should().Contain($"\"ordemServicoId\":{ordemServicoId}");
        consulta.StatusCode.Should().Be(HttpStatusCode.OK);
        primeira.StatusCode.Should().Be(HttpStatusCode.OK);
        repetida.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = JsonDocument.Parse(await primeira.Content.ReadAsStringAsync());
        json.RootElement.GetProperty("status").GetString().Should().Be("Finalizada");

        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ExecucaoDbContext>();
        (await context.OutboxMessages.CountAsync(
            item => item.Tipo == "ExecucaoFinalizadaMessage" &&
                    item.CorrelationId == context.ExecucoesReparo
                        .Where(execucao => execucao.OrdemServicoId == ordemServicoId)
                        .Select(execucao => execucao.CorrelationId)
                        .Single()))
            .Should().Be(1);
    }

    [Fact]
    public async Task Consultar_DeveRetornarNotFoundQuandoReparoNaoExiste()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/v1/execucoes/ordem-servico/999999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Repository_DeveCriarReutilizarERejeitarComandoIncompativel()
    {
        const int ordemServicoId = 3902;
        var correlationId = Guid.NewGuid();
        var orcamentoId = Guid.NewGuid();
        using var scope = factory.Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IExecucaoReparoRepository>();
        var context = scope.ServiceProvider.GetRequiredService<ExecucaoDbContext>();

        var criada = await repository.CriarOuObterAsync(
            correlationId, ordemServicoId, 2, orcamentoId,
            DateTimeOffset.UtcNow, CancellationToken.None);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
        var repetida = await repository.CriarOuObterAsync(
            correlationId, ordemServicoId, 2, orcamentoId,
            DateTimeOffset.UtcNow.AddMinutes(1), CancellationToken.None);
        var incompativel = () => repository.CriarOuObterAsync(
            Guid.NewGuid(), ordemServicoId, 2, orcamentoId,
            DateTimeOffset.UtcNow, CancellationToken.None);

        repetida.Id.Should().Be(criada.Id);
        await incompativel.Should().ThrowAsync<InvalidOperationException>();
    }

    private async Task CriarExecucaoAsync(int ordemServicoId)
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ExecucaoDbContext>();
        if (await context.ExecucoesReparo.AnyAsync(item => item.OrdemServicoId == ordemServicoId))
            return;
        context.ExecucoesReparo.Add(ExecucaoReparo.Criar(
            Guid.NewGuid(), ordemServicoId, 1, Guid.NewGuid(), DateTimeOffset.UtcNow));
        await context.SaveChangesAsync();
    }
}
