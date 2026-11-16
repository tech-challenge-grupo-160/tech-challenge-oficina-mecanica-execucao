using FluentValidation;

namespace Fiap.TechChallenge.OficinaMecanica.Execucao.Application.Estoque;

public sealed class ReporEstoqueCommandValidator : AbstractValidator<ReporEstoqueCommand>
{
    public ReporEstoqueCommandValidator()
    {
        RuleFor(command => command.PecaId).GreaterThan(0);
        RuleFor(command => command.Quantidade).GreaterThan(0);
    }
}
