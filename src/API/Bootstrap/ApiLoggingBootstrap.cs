using Fiap.TechChallenge.OficinaMecanica.Execucao.Infrastructure.Logging;

namespace Fiap.TechChallenge.OficinaMecanica.Execucao.API.Bootstrap;

public static class ApiLoggingBootstrap
{
    public static WebApplicationBuilder ConfigureApiLogging(this WebApplicationBuilder builder)
    {
        builder.Logging.ClearProviders();
        builder.Logging.AddConsoleFormatter<JsonConsoleFormatter, JsonConsoleFormatterOptions>();
        builder.Logging.AddConsole(options =>
        {
            options.FormatterName = JsonConsoleFormatter.FormatterName;
        });
        builder.Logging.Services.Configure<JsonConsoleFormatterOptions>(options =>
        {
            options.UseUtcTimestamp = true;
            options.ServiceName = builder.Configuration["Logging:ServiceName"] ?? "oficina-mecanica-execucao";
            options.EnvironmentName = builder.Environment.EnvironmentName;
        });

        return builder;
    }
}
