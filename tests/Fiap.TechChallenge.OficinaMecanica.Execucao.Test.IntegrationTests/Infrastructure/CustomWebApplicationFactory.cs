using Fiap.TechChallenge.OficinaMecanica.Execucao.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.PostgreSql;

namespace Fiap.TechChallenge.OficinaMecanica.Execucao.Test.IntegrationTests.Infrastructure;

public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const int ServicoExistenteId = 1000;
    public const int PecaExistenteId = 1000;

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine")
        .WithDatabase("oficina_execucao_tests")
        .WithUsername("postgres")
        .WithPassword(Guid.NewGuid().ToString("N"))
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("IntegrationTesting");
        builder.ConfigureAppConfiguration((_, configuration) =>
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = _postgres.GetConnectionString()
            }));
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<DbContextOptions<ExecucaoDbContext>>();
            services.RemoveAll<ExecucaoDbContext>();
            services.AddDbContext<ExecucaoDbContext>(options =>
                options.UseNpgsql(_postgres.GetConnectionString()));
        });
    }

    async Task IAsyncLifetime.InitializeAsync()
    {
        await _postgres.StartAsync();
        using var client = CreateClient();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        Dispose();
        await _postgres.DisposeAsync();
    }
}
