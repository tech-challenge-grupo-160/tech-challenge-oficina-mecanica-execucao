using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Fiap.TechChallenge.OficinaMecanica.Execucao.Infrastructure.Data;
using Fiap.TechChallenge.OficinaMecanica.Execucao.Test.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Fiap.TechChallenge.OficinaMecanica.Execucao.Test.IntegrationTests.Endpoints.Diagnosticos;

public sealed class DiagnosticosControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public DiagnosticosControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Iniciar_DeveCriarEReutilizarMesmoDiagnostico()
    {
        var payload = CriarPayload(ordemServicoId: 3001, veiculoId: 1001);

        var primeira = await EnviarAsync(payload, "iniciar-diagnostico:3001");
        var repetida = await EnviarAsync(payload, "iniciar-diagnostico:3001");

        primeira.StatusCode.Should().Be(HttpStatusCode.Created);
        repetida.StatusCode.Should().Be(HttpStatusCode.OK);
        var primeiraJson = JsonDocument.Parse(await primeira.Content.ReadAsStringAsync());
        var repetidaJson = JsonDocument.Parse(await repetida.Content.ReadAsStringAsync());
        repetidaJson.RootElement.GetProperty("id").GetInt32()
            .Should().Be(primeiraJson.RootElement.GetProperty("id").GetInt32());
        primeiraJson.RootElement.GetProperty("status").GetString().Should().Be("EmDiagnostico");

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ExecucaoDbContext>();
        (await context.Diagnosticos.CountAsync(item => item.OrdemServicoId == 3001)).Should().Be(1);
    }

    [Fact]
    public async Task Iniciar_DeveRetornarConflitoQuandoRepeticaoMudarPayload()
    {
        await EnviarAsync(CriarPayload(3002, 1001), "iniciar-diagnostico:3002");

        var response = await EnviarAsync(CriarPayload(3002, 1002), "iniciar-diagnostico:3002");

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Iniciar_DeveExigirChaveDeIdempotencia()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/v1/diagnosticos",
            CriarPayload(3003, 1001));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ObterPorOrdem_DeveRetornarDiagnosticoCriado()
    {
        await EnviarAsync(CriarPayload(3004, 1001), "iniciar-diagnostico:3004");

        var response = await _client.GetAsync("/api/v1/diagnosticos/ordem-servico/3004");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        json.RootElement.GetProperty("ordemServicoId").GetInt32().Should().Be(3004);
    }

    [Fact]
    public async Task Iniciar_DeveSerIdempotenteSobRequisicoesConcorrentes()
    {
        var requisicoes = Enumerable.Range(0, 5)
            .Select(_ => EnviarAsync(
                CriarPayload(3005, 1001),
                "iniciar-diagnostico:3005"));

        var respostas = await Task.WhenAll(requisicoes);

        respostas.Should().ContainSingle(response => response.StatusCode == HttpStatusCode.Created);
        respostas.Should().OnlyContain(response =>
            response.StatusCode == HttpStatusCode.Created || response.StatusCode == HttpStatusCode.OK);
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ExecucaoDbContext>();
        (await context.Diagnosticos.CountAsync(item => item.OrdemServicoId == 3005)).Should().Be(1);
    }

    [Fact]
    public async Task Iniciar_DeveRejeitarServicoInexistente()
    {
        var payload = new
        {
            ordemServicoId = 3006,
            veiculoId = 1001,
            descricaoSolicitacao = "Ruido durante a frenagem.",
            servicoIdsSolicitados = new[] { 999999 },
            pecaIdsSolicitadas = Array.Empty<int>()
        };

        var response = await EnviarAsync(payload, "iniciar-diagnostico:3006");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Iniciar_DeveRejeitarColecaoDePecasNula()
    {
        var payload = new
        {
            ordemServicoId = 3007,
            veiculoId = 1001,
            descricaoSolicitacao = "Ruido durante a frenagem.",
            servicoIdsSolicitados = new[] { CustomWebApplicationFactory.ServicoExistenteId },
            pecaIdsSolicitadas = (int[]?)null
        };

        var response = await EnviarAsync(payload, "iniciar-diagnostico:3007");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Iniciar_DeveRetornarConflitoQuandoOrdemEChavePertenceremADiagnosticosDiferentes()
    {
        await EnviarAsync(CriarPayload(3008, 1001), "iniciar-diagnostico:3008");
        await EnviarAsync(CriarPayload(3009, 1002), "iniciar-diagnostico:3009");

        var response = await EnviarAsync(
            CriarPayload(3008, 1001),
            "iniciar-diagnostico:3009");

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    private async Task<HttpResponseMessage> EnviarAsync(object payload, string chave)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/diagnosticos")
        {
            Content = JsonContent.Create(payload)
        };
        request.Headers.Add("Idempotency-Key", chave);
        return await _client.SendAsync(request);
    }

    private static object CriarPayload(int ordemServicoId, int veiculoId) => new
    {
        ordemServicoId,
        veiculoId,
        descricaoSolicitacao = "Ruido durante a frenagem.",
        servicoIdsSolicitados = new[] { CustomWebApplicationFactory.ServicoExistenteId },
        pecaIdsSolicitadas = new[] { CustomWebApplicationFactory.PecaExistenteId }
    };
}
