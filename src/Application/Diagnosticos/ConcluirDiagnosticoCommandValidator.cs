using FluentValidation;

namespace Fiap.TechChallenge.OficinaMecanica.Execucao.Application.Diagnosticos;

public sealed class ConcluirDiagnosticoCommandValidator : AbstractValidator<ConcluirDiagnosticoCommand>
{
    public ConcluirDiagnosticoCommandValidator()
    {
        RuleFor(command => command.OrdemServicoId).GreaterThan(0);
    }
}
