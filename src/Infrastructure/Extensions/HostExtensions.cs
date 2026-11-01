using Fiap.TechChallenge.OficinaMecanica.Execucao.Infrastructure.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Fiap.TechChallenge.OficinaMecanica.Execucao.Infrastructure;

public static class HostExtensions
{
    public static async Task MigrateDatabaseAsync(this WebApplication app)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<ExecucaoDbContext>();
        await context.Database.MigrateAsync();
    }
}
