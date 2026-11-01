using Fiap.TechChallenge.OficinaMecanica.Execucao.Application.Abstractions;
using Fiap.TechChallenge.OficinaMecanica.Execucao.Infrastructure.Data;
using Fiap.TechChallenge.OficinaMecanica.Execucao.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Fiap.TechChallenge.OficinaMecanica.Execucao.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Host=localhost;Port=5433;Database=oficina_execucao;Username=postgres;Password=postgres";

        services.AddDbContext<ExecucaoDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<ICatalogoRepository, CatalogoRepository>();
        services
            .AddHealthChecks()
            .AddDbContextCheck<ExecucaoDbContext>(
                name: "Database",
                failureStatus: HealthStatus.Unhealthy);

        return services;
    }
}
