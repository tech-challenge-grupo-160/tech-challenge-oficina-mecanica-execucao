using Fiap.TechChallenge.OficinaMecanica.Execucao.Infrastructure.Data;
using Fiap.TechChallenge.OficinaMecanica.Execucao.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Fiap.TechChallenge.OficinaMecanica.Execucao.Test.IntegrationTests.Infrastructure;

public sealed class ReservaEstoqueRepositoryTests(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>
{
    [Fact]
    public async Task Reservar_DevePreservarSaldoInsuficienteEManterIdempotencia()
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<ExecucaoDbContext>();
        await using var transaction = await context.Database.BeginTransactionAsync();
        var peca = await context.Pecas.SingleAsync(item => item.Id == 1000);
        var estoqueInicial = peca.QuantidadeEstoque;
        var solicitada = estoqueInicial + 2;
        var correlationId = Guid.NewGuid();
        var repository = new ReservaEstoqueRepository(context);

        var primeira = await repository.ReservarAsync(
            correlationId, 99001, [(1000, solicitada)],
            DateTimeOffset.UtcNow, CancellationToken.None);
        await context.SaveChangesAsync();
        var segunda = await repository.ReservarAsync(
            correlationId, 99001, [(1000, solicitada)],
            DateTimeOffset.UtcNow, CancellationToken.None);

        primeira.Status.Should().Be("EstoqueInsuficiente");
        primeira.Itens.Single().QuantidadeReservada.Should().Be(0);
        primeira.Itens.Single().QuantidadeFaltante.Should().Be(solicitada);
        segunda.Id.Should().Be(primeira.Id);
        peca.QuantidadeEstoque.Should().Be(estoqueInicial);
        await transaction.RollbackAsync();
    }
}
