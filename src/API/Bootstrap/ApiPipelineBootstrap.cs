using Fiap.TechChallenge.OficinaMecanica.Execucao.API.Middleware;
using Fiap.TechChallenge.OficinaMecanica.Execucao.Infrastructure.HealthChecks;

namespace Fiap.TechChallenge.OficinaMecanica.Execucao.API.Bootstrap;

public static class ApiPipelineBootstrap
{
    public static WebApplication UseApiPipeline(this WebApplication app, IConfiguration configuration)
    {
        app.UseApiSwagger();

        var configuredUrls = configuration["ASPNETCORE_URLS"] ?? string.Empty;
        var hasHttpsBinding = configuredUrls.Contains("https://", StringComparison.OrdinalIgnoreCase);

        if (hasHttpsBinding)
        {
            app.UseHttpsRedirection();
        }

        app.UseRouting();
        app.UseMiddleware<RequestLoggingMiddleware>();
        app.MapControllers();
        app.UseHealthChecks();

        return app;
    }
}
