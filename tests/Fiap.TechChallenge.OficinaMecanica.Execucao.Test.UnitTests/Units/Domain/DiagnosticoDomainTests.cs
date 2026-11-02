using Fiap.TechChallenge.OficinaMecanica.Execucao.Domain.Entities;
using FluentAssertions;

namespace Fiap.TechChallenge.OficinaMecanica.Execucao.Test.UnitTests.Units.Domain;

public sealed class DiagnosticoDomainTests
{
    [Fact]
    public void Iniciar_DeveNormalizarServicosEDefinirStatus()
    {
        var iniciadoEm = new DateTimeOffset(2026, 11, 1, 18, 0, 0, TimeSpan.Zero);

        var diagnostico = Diagnostico.Iniciar(
            3001, 1001, " Ruido nos freios ", [1001, 1000, 1001], [2000, 2000],
            "iniciar-diagnostico:3001", iniciadoEm);

        diagnostico.OrdemServicoId.Should().Be(3001);
        diagnostico.ServicoIdsSolicitados.Should().Equal(1000, 1001);
        diagnostico.PecaIdsSolicitadas.Should().Equal(2000);
        diagnostico.DescricaoSolicitacao.Should().Be("Ruido nos freios");
        diagnostico.Status.Should().Be("EmDiagnostico");
        diagnostico.IniciadoEm.Should().Be(iniciadoEm);
    }

    [Fact]
    public void Iniciar_DeveRejeitarOrdemSemServicoSolicitado()
    {
        var action = () => Diagnostico.Iniciar(
            3001, 1001, "Ruido", [], [], "iniciar-diagnostico:3001", DateTimeOffset.UtcNow);

        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void CorrespondeA_DeveDetectarRepeticaoIdempotente()
    {
        var primeiro = Diagnostico.Iniciar(
            3001, 1001, "Ruido", [1000], [2000], "iniciar-diagnostico:3001", DateTimeOffset.UtcNow);
        var repetido = Diagnostico.Iniciar(
            3001, 1001, "Ruido", [1000], [2000], "iniciar-diagnostico:3001", DateTimeOffset.UtcNow.AddMinutes(1));

        primeiro.CorrespondeA(repetido).Should().BeTrue();
    }

    [Fact]
    public void RegistrarResultado_DeveManterDiagnosticoEmAndamentoEArmazenarItens()
    {
        var diagnostico = CriarDiagnostico();
        var atualizadoEm = new DateTimeOffset(2026, 11, 1, 22, 0, 0, TimeSpan.Zero);

        var alterado = diagnostico.RegistrarResultado(
            " Pastilhas desgastadas e veiculo desalinhado. ",
            [1001, 1000],
            [(2000, 2)],
            atualizadoEm);

        alterado.Should().BeTrue();
        diagnostico.Status.Should().Be("EmDiagnostico");
        diagnostico.ParecerTecnico.Should().Be("Pastilhas desgastadas e veiculo desalinhado.");
        diagnostico.ServicosResultado.Select(item => item.ServicoId).Should().Equal(1000, 1001);
        diagnostico.PecasResultado.Should().ContainSingle()
            .Which.Should().BeEquivalentTo(new { PecaId = 2000, Quantidade = 2 });
        diagnostico.ResultadoAtualizadoEm.Should().Be(atualizadoEm);
        diagnostico.Versao.Should().Be(1);
    }

    [Fact]
    public void RegistrarResultado_DeveSerIdempotenteParaMesmoConteudo()
    {
        var diagnostico = CriarDiagnostico();
        var primeiraData = new DateTimeOffset(2026, 11, 1, 22, 0, 0, TimeSpan.Zero);
        diagnostico.RegistrarResultado("Parecer", [1000], [(2000, 2)], primeiraData);

        var alterado = diagnostico.RegistrarResultado(
            "Parecer", [1000], [(2000, 2)], primeiraData.AddMinutes(1));

        alterado.Should().BeFalse();
        diagnostico.ResultadoAtualizadoEm.Should().Be(primeiraData);
        diagnostico.Versao.Should().Be(1);
    }

    [Fact]
    public void RegistrarResultado_DeveRejeitarServicoDuplicado()
    {
        var diagnostico = CriarDiagnostico();

        var action = () => diagnostico.RegistrarResultado(
            "Parecer", [1000, 1000], [], DateTimeOffset.UtcNow);

        action.Should().Throw<ArgumentException>();
    }

    private static Diagnostico CriarDiagnostico() => Diagnostico.Iniciar(
        3001,
        1001,
        "Ruido",
        [1000],
        [2000],
        "iniciar-diagnostico:3001",
        DateTimeOffset.UtcNow);
}
