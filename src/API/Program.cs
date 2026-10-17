using Fiap.TechChallenge.OficinaMecanica.Execucao.API.Bootstrap;
using Fiap.TechChallenge.OficinaMecanica.Execucao.Application;
using Fiap.TechChallenge.OficinaMecanica.Execucao.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureApiLogging();

builder.Services.AddApiServices();
builder.Services.AddApplication();
builder.Services.AddInfrastructure();

var app = builder.Build();

app.UseApiPipeline(builder.Configuration);
app.LogStartupUrls();

app.Run();

public partial class Program;
