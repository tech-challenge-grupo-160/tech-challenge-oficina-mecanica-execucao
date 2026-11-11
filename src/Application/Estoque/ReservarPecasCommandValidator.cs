using FluentValidation;

namespace Fiap.TechChallenge.OficinaMecanica.Execucao.Application.Estoque;

public sealed class ReservarPecasCommandValidator : AbstractValidator<ReservarPecasCommand>
{
    public ReservarPecasCommandValidator()
    {
        RuleFor(command => command.CorrelationId).NotEmpty();
        RuleFor(command => command.OrdemServicoId).GreaterThan(0);
        RuleFor(command => command.OrcamentoId).NotEmpty();
        RuleFor(command => command.Pecas).NotEmpty();
        RuleFor(command => command.Pecas)
            .Must(pecas => pecas.Select(item => item.PecaId).Distinct().Count() == pecas.Count)
            .WithMessage("Nao e permitido repetir pecas na reserva.");
        RuleForEach(command => command.Pecas).ChildRules(peca =>
        {
            peca.RuleFor(item => item.PecaId).GreaterThan(0);
            peca.RuleFor(item => item.Quantidade).GreaterThan(0);
        });
    }
}
