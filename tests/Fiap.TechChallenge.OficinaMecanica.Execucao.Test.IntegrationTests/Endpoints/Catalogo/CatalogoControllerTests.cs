using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Fiap.TechChallenge.OficinaMecanica.Execucao.Test.IntegrationTests.Infrastructure;
using FluentAssertions;

namespace Fiap.TechChallenge.OficinaMecanica.Execucao.Test.IntegrationTests.Endpoints.Catalogo;

public sealed class CatalogoControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public CatalogoControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Consultar_DeveRetornarSnapshotsSemExporEstoque()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/catalogo/itens/consultar", new
        {
            servicoIds = new[] { CustomWebApplicationFactory.ServicoExistenteId },
            pecaIds = new[] { CustomWebApplicationFactory.PecaExistenteId }
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = json.RootElement;
        var servico = root.GetProperty("servicos")[0];
        servico.GetProperty("id").GetInt32().Should().Be(1000);
        servico.GetProperty("preco").GetDecimal().Should().Be(150m);
        servico.GetProperty("tempoEstimado").GetInt32().Should().Be(30);
        var peca = root.GetProperty("pecas")[0];
        peca.GetProperty("id").GetInt32().Should().Be(1000);
        peca.GetProperty("preco").GetDecimal().Should().Be(45m);
        peca.TryGetProperty("quantidadeEstoque", out _).Should().BeFalse();
        root.GetProperty("servicoIdsNaoEncontrados").GetArrayLength().Should().Be(0);
        root.GetProperty("pecaIdsNaoEncontrados").GetArrayLength().Should().Be(0);
    }

    [Fact]
    public async Task Consultar_DeveDisponibilizarCatalogoInicialCompleto()
    {
        var ids = Enumerable.Range(1000, 8).ToArray();

        var response = await _client.PostAsJsonAsync("/api/v1/catalogo/itens/consultar", new
        {
            servicoIds = ids,
            pecaIds = ids
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        json.RootElement.GetProperty("servicos").GetArrayLength().Should().Be(8);
        json.RootElement.GetProperty("pecas").GetArrayLength().Should().Be(8);
        json.RootElement.GetProperty("servicoIdsNaoEncontrados").GetArrayLength().Should().Be(0);
        json.RootElement.GetProperty("pecaIdsNaoEncontrados").GetArrayLength().Should().Be(0);
    }

    [Fact]
    public async Task Consultar_DeveInformarIdsNaoEncontrados()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/catalogo/itens/consultar", new
        {
            servicoIds = new[] { 1000, 999999 },
            pecaIds = new[] { 888888 }
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        json.RootElement.GetProperty("servicos").GetArrayLength().Should().Be(1);
        json.RootElement.GetProperty("pecas").GetArrayLength().Should().Be(0);
        json.RootElement.GetProperty("servicoIdsNaoEncontrados")[0].GetInt32().Should().Be(999999);
        json.RootElement.GetProperty("pecaIdsNaoEncontrados")[0].GetInt32().Should().Be(888888);
    }

    [Theory]
    [InlineData("{\"servicoIds\":[],\"pecaIds\":[]}")]
    [InlineData("{\"servicoIds\":null,\"pecaIds\":null}")]
    [InlineData("{\"servicoIds\":[0],\"pecaIds\":[]}")]
    [InlineData("{\"servicoIds\":[],\"pecaIds\":[-1]}")]
    public async Task Consultar_DeveRejeitarRequisicaoInvalida(string payload)
    {
        using var content = new StringContent(payload, System.Text.Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/v1/catalogo/itens/consultar", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");
    }
}
