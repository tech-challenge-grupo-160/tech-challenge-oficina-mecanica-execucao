using FluentValidation;

namespace Fiap.TechChallenge.OficinaMecanica.Execucao.Application.Diagnosticos;

public sealed class RegistrarResultadoDiagnosticoCommandValidator
    : AbstractValidator<RegistrarResultadoDiagnosticoCommand>
{
    public RegistrarResultadoDiagnosticoCommandValidator()
    {
        RuleFor(command => command.OrdemServicoId).GreaterThan(0);
        RuleFor(command => command.ParecerTecnico).NotEmpty().MaximumLength(2000);
        RuleFor(command => command.ServicoIds).NotEmpty();
        RuleForEach(command => command.ServicoIds).GreaterThan(0);
        RuleFor(command => command.Pecas).NotNull();
        RuleForEach(command => command.Pecas).ChildRules(peca =>
        {
            peca.RuleFor(item => item.PecaId).GreaterThan(0);
            peca.RuleFor(item => item.Quantidade).GreaterThan(0);
        });
    }
}
