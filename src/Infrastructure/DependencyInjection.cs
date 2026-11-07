using Fiap.TechChallenge.OficinaMecanica.Execucao.Application.Abstractions;
using Fiap.TechChallenge.OficinaMecanica.Execucao.Infrastructure.Data;
using Fiap.TechChallenge.OficinaMecanica.Execucao.Infrastructure.Repositories;
using Fiap.TechChallenge.OficinaMecanica.Execucao.Infrastructure.Messaging;
using Amazon.SimpleNotificationService;
using Amazon.SQS;
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
            ?? throw new InvalidOperationException(
                "A connection string 'DefaultConnection' nao foi configurada.");

        services.AddDbContext<ExecucaoDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<ICatalogoRepository, CatalogoRepository>();
        services.AddScoped<IDiagnosticoRepository, DiagnosticoRepository>();
        services.AddScoped<IReservaEstoqueRepository, ReservaEstoqueRepository>();
        services.AddScoped<IExecucaoReparoRepository, ExecucaoReparoRepository>();
        var messagingOptions = configuration
            .GetSection(MessagingOptions.SectionName)
            .Get<MessagingOptions>() ?? new MessagingOptions();
        services.Configure<MessagingOptions>(
            configuration.GetSection(MessagingOptions.SectionName));
        services.AddSingleton<IAmazonSQS>(_ => AwsMessagingClientFactory.CreateSqs(messagingOptions));
        services.AddSingleton<IAmazonSimpleNotificationService>(_ =>
            AwsMessagingClientFactory.CreateSns(messagingOptions));
        services.AddHostedService<InicioDiagnosticoConsumerWorker>();
        services.AddHostedService<ReservaPecasConsumerWorker>();
        services.AddHostedService<InicioExecucaoConsumerWorker>();
        services.AddHostedService<OutboxSnsPublisherWorker>();
        services
            .AddHealthChecks()
            .AddDbContextCheck<ExecucaoDbContext>(
                name: "Database",
                failureStatus: HealthStatus.Unhealthy);

        return services;
    }
}
