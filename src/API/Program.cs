using Fiap.TechChallenge.OficinaMecanica.Execucao.API.Bootstrap;
using Fiap.TechChallenge.OficinaMecanica.Execucao.Application;
using Fiap.TechChallenge.OficinaMecanica.Execucao.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureApiLogging();

builder.Services.AddApiServices();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

await app.MigrateDatabaseAsync();

app.UseApiPipeline(builder.Configuration);
app.LogStartupUrls();

app.Run();

public partial class Program;
