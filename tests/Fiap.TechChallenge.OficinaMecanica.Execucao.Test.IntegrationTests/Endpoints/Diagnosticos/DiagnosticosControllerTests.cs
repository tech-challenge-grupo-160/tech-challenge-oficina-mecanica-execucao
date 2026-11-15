using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Fiap.TechChallenge.OficinaMecanica.Execucao.Domain.Entities;
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

    [Fact]
    public async Task Resultado_DeveRegistrarConsultarEManterDiagnosticoEmAndamento()
    {
        await EnviarAsync(CriarPayload(3010, 1001), "iniciar-diagnostico:3010");

        var registro = await _client.PutAsJsonAsync(
            "/api/v1/diagnosticos/ordem-servico/3010/resultado",
            CriarResultadoPayload());
        var consulta = await _client.GetAsync(
            "/api/v1/diagnosticos/ordem-servico/3010/resultado");
        var diagnostico = await _client.GetAsync(
            "/api/v1/diagnosticos/ordem-servico/3010");

        registro.StatusCode.Should().Be(HttpStatusCode.OK);
        consulta.StatusCode.Should().Be(HttpStatusCode.OK);
        var resultadoJson = JsonDocument.Parse(await consulta.Content.ReadAsStringAsync());
        resultadoJson.RootElement.GetProperty("parecerTecnico").GetString()
            .Should().Be("Pastilhas desgastadas e veiculo desalinhado.");
        resultadoJson.RootElement.GetProperty("versao").GetInt32().Should().Be(1);
        resultadoJson.RootElement.GetProperty("pecas")[0].GetProperty("quantidade").GetInt32()
            .Should().Be(2);
        var diagnosticoJson = JsonDocument.Parse(await diagnostico.Content.ReadAsStringAsync());
        diagnosticoJson.RootElement.GetProperty("status").GetString().Should().Be("EmDiagnostico");
    }

    [Fact]
    public async Task Resultado_DeveSerIdempotenteParaMesmoConteudo()
    {
        await EnviarAsync(CriarPayload(3011, 1001), "iniciar-diagnostico:3011");
        var primeira = await _client.PutAsJsonAsync(
            "/api/v1/diagnosticos/ordem-servico/3011/resultado",
            CriarResultadoPayload());
        var repetida = await _client.PutAsJsonAsync(
            "/api/v1/diagnosticos/ordem-servico/3011/resultado",
            CriarResultadoPayload());

        primeira.StatusCode.Should().Be(HttpStatusCode.OK);
        repetida.StatusCode.Should().Be(HttpStatusCode.OK);
        var primeiraJson = JsonDocument.Parse(await primeira.Content.ReadAsStringAsync());
        var repetidaJson = JsonDocument.Parse(await repetida.Content.ReadAsStringAsync());
        repetidaJson.RootElement.GetProperty("versao").GetInt32().Should().Be(1);
        repetidaJson.RootElement.GetProperty("atualizadoEm").GetDateTimeOffset()
            .Should().Be(primeiraJson.RootElement.GetProperty("atualizadoEm").GetDateTimeOffset());

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ExecucaoDbContext>();
        (await context.Set<DiagnosticoServicoResultado>()
                .CountAsync(item => item.DiagnosticoId == primeiraJson.RootElement
                    .GetProperty("diagnosticoId").GetInt32()))
            .Should().Be(1);
        (await context.Set<DiagnosticoPecaResultado>()
                .CountAsync(item => item.DiagnosticoId == primeiraJson.RootElement
                    .GetProperty("diagnosticoId").GetInt32()))
            .Should().Be(1);
    }

    [Fact]
    public async Task ObterResultado_DeveRetornarNaoEncontradoAntesDoRegistro()
    {
        await EnviarAsync(CriarPayload(3012, 1001), "iniciar-diagnostico:3012");

        var response = await _client.GetAsync(
            "/api/v1/diagnosticos/ordem-servico/3012/resultado");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Resultado_DeveRejeitarItemInexistente()
    {
        await EnviarAsync(CriarPayload(3013, 1001), "iniciar-diagnostico:3013");
        var payload = new
        {
            parecerTecnico = "Item inexistente.",
            servicoIds = new[] { 999999 },
            pecas = Array.Empty<object>()
        };

        var response = await _client.PutAsJsonAsync(
            "/api/v1/diagnosticos/ordem-servico/3013/resultado",
            payload);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Resultado_DeveSubstituirRepresentacaoAnteriorEIncrementarVersao()
    {
        await EnviarAsync(CriarPayload(3014, 1001), "iniciar-diagnostico:3014");
        await _client.PutAsJsonAsync(
            "/api/v1/diagnosticos/ordem-servico/3014/resultado",
            CriarResultadoPayload());
        var alterado = await _client.PutAsJsonAsync(
            "/api/v1/diagnosticos/ordem-servico/3014/resultado",
            new
            {
                parecerTecnico = "Parecer tecnico revisado.",
                servicoIds = new[] { CustomWebApplicationFactory.ServicoExistenteId },
                pecas = new[]
                {
                    new { pecaId = CustomWebApplicationFactory.PecaExistenteId, quantidade = 4 }
                }
            });

        alterado.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = JsonDocument.Parse(await alterado.Content.ReadAsStringAsync());
        json.RootElement.GetProperty("versao").GetInt32().Should().Be(2);
        json.RootElement.GetProperty("parecerTecnico").GetString()
            .Should().Be("Parecer tecnico revisado.");
        json.RootElement.GetProperty("pecas")[0].GetProperty("quantidade").GetInt32()
            .Should().Be(4);
    }

    [Fact]
    public async Task Concluir_DeveExigirResultadoTecnico()
    {
        await EnviarAsync(CriarPayload(3015, 1001), "iniciar-diagnostico:3015");

        var response = await _client.PatchAsync(
            "/api/v1/diagnosticos/ordem-servico/3015/concluir",
            null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Concluir_DevePersistirEventoUmaUnicaVez()
    {
        await EnviarAsync(CriarPayload(3016, 1001), "iniciar-diagnostico:3016");
        await _client.PutAsJsonAsync(
            "/api/v1/diagnosticos/ordem-servico/3016/resultado",
            CriarResultadoPayload());

        var primeira = await _client.PatchAsync(
            "/api/v1/diagnosticos/ordem-servico/3016/concluir",
            null);
        var repetida = await _client.PatchAsync(
            "/api/v1/diagnosticos/ordem-servico/3016/concluir",
            null);

        primeira.StatusCode.Should().Be(HttpStatusCode.OK);
        repetida.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = JsonDocument.Parse(await primeira.Content.ReadAsStringAsync());
        json.RootElement.GetProperty("status").GetString().Should().Be("Concluido");
        json.RootElement.GetProperty("servicos").GetArrayLength().Should().Be(1);
        json.RootElement.GetProperty("pecas")[0].GetProperty("quantidade").GetInt32().Should().Be(2);

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ExecucaoDbContext>();
        (await context.OutboxMessages.CountAsync(item =>
                item.Tipo == "DiagnosticoConcluidoMessage"))
            .Should().Be(1);
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

    private static object CriarResultadoPayload() => new
    {
        parecerTecnico = "Pastilhas desgastadas e veiculo desalinhado.",
        servicoIds = new[] { CustomWebApplicationFactory.ServicoExistenteId },
        pecas = new[]
        {
            new { pecaId = CustomWebApplicationFactory.PecaExistenteId, quantidade = 2 }
        }
    };
}
