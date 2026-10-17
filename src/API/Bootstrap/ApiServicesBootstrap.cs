using Fiap.TechChallenge.OficinaMecanica.Execucao.API.Filters;
using Fiap.TechChallenge.OficinaMecanica.Execucao.API.ProblemDetails;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;

namespace Fiap.TechChallenge.OficinaMecanica.Execucao.API.Bootstrap;

// Autenticacao JWT entra junto com o primeiro endpoint protegido do servico.
public static class ApiServicesBootstrap
{
    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        services.AddControllers(options =>
        {
            options.Filters.Add<DomainExceptionFilter>();
        });
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var result = new BadRequestObjectResult(
                    ApiProblemDetails.CreateValidation(context.HttpContext, context.ModelState));
                result.ContentTypes.Add(ApiProblemDetails.ContentType);

                return result;
            };
        });
        services.AddFluentValidationAutoValidation();
        services.AddEndpointsApiExplorer();
        services.AddApiSwagger();

        return services;
    }
}
