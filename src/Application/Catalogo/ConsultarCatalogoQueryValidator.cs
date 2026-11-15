using FluentValidation;

namespace Fiap.TechChallenge.OficinaMecanica.Execucao.Application.Catalogo;

public sealed class ConsultarCatalogoQueryValidator : AbstractValidator<ConsultarCatalogoQuery>
{
    public ConsultarCatalogoQueryValidator()
    {
        RuleFor(query => query)
            .Must(query => query.ServicoIds.Count > 0 || query.PecaIds.Count > 0)
            .WithMessage("Informe ao menos um servico ou uma peca.");
        RuleForEach(query => query.ServicoIds)
            .GreaterThan(0)
            .WithMessage("Os IDs de servico devem ser maiores que zero.");
        RuleForEach(query => query.PecaIds)
            .GreaterThan(0)
            .WithMessage("Os IDs de peca devem ser maiores que zero.");
    }
}
