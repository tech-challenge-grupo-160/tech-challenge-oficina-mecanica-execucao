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
}
