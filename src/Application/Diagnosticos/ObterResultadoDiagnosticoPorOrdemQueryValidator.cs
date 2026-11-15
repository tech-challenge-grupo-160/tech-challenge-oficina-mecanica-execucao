using FluentValidation;

namespace Fiap.TechChallenge.OficinaMecanica.Execucao.Application.Diagnosticos;

public sealed class ObterResultadoDiagnosticoPorOrdemQueryValidator
    : AbstractValidator<ObterResultadoDiagnosticoPorOrdemQuery>
{
    public ObterResultadoDiagnosticoPorOrdemQueryValidator()
    {
        RuleFor(query => query.OrdemServicoId).GreaterThan(0);
    }
}
