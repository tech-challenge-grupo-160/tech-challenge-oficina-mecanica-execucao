using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Fiap.TechChallenge.OficinaMecanica.Execucao.Infrastructure.Data;
using Fiap.TechChallenge.OficinaMecanica.Execucao.Infrastructure.Repositories;
using Fiap.TechChallenge.OficinaMecanica.Execucao.Test.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Fiap.TechChallenge.OficinaMecanica.Execucao.Test.IntegrationTests.Endpoints.Estoque;

public sealed class EstoqueControllerTests(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>
{
    [Fact]
    public async Task Repor_DeveCompletarReservaPendenteEmFifoEGravarEventoNaOutbox()
    {
        const int pecaId = 1007;
        const int ordemServicoId = 3971;
        int quantidadeNecessaria;
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ExecucaoDbContext>();
            quantidadeNecessaria = (await context.Pecas.AsNoTracking()
                .SingleAsync(item => item.Id == pecaId)).QuantidadeEstoque + 3;
            var repository = new ReservaEstoqueRepository(context);
            await repository.ReservarAsync(
                Guid.NewGuid(), ordemServicoId, Guid.NewGuid(),
                [(pecaId, quantidadeNecessaria)], DateTimeOffset.UtcNow,
                CancellationToken.None);
            await context.SaveChangesAsync();
        }

        using var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync(
            $"/api/v1/estoque/pecas/{pecaId}/reposicoes",
            new { quantidade = 3 });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        json.RootElement.GetProperty("quantidadeEstoque").GetInt32().Should().Be(0);
        json.RootElement.GetProperty("ordensServicoLiberadas")[0].GetInt32()
            .Should().Be(ordemServicoId);

        await using var assertionScope = factory.Services.CreateAsyncScope();
        var assertionContext = assertionScope.ServiceProvider.GetRequiredService<ExecucaoDbContext>();
        var reserva = await assertionContext.ReservasEstoque
            .Include(item => item.Itens)
            .SingleAsync(item => item.OrdemServicoId == ordemServicoId);
        reserva.Status.Should().Be("Reservada");
        reserva.Itens.Single().QuantidadeReservada.Should().Be(quantidadeNecessaria);
        (await assertionContext.OutboxMessages.CountAsync(item =>
            item.Id == reserva.EventoResultadoId && item.Tipo == "PecasReservadasMessage"))
            .Should().Be(1);
    }

    [Theory]
    [InlineData(1007, 0, HttpStatusCode.BadRequest)]
    [InlineData(999999, 1, HttpStatusCode.NotFound)]
    public async Task Repor_DeveValidarEntradaEPecaExistente(
        int pecaId,
        int quantidade,
        HttpStatusCode statusEsperado)
    {
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            $"/api/v1/estoque/pecas/{pecaId}/reposicoes",
            new { quantidade });

        response.StatusCode.Should().Be(statusEsperado);
    }
}
