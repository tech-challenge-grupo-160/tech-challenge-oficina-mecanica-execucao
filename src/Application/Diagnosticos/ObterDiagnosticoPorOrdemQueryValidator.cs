using FluentValidation;

namespace Fiap.TechChallenge.OficinaMecanica.Execucao.Application.Diagnosticos;

public sealed class ObterDiagnosticoPorOrdemQueryValidator
    : AbstractValidator<ObterDiagnosticoPorOrdemQuery>
{
    public ObterDiagnosticoPorOrdemQueryValidator()
    {
        RuleFor(query => query.OrdemServicoId).GreaterThan(0);
    }
}
