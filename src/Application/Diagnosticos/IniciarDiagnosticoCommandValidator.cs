using FluentValidation;

namespace Fiap.TechChallenge.OficinaMecanica.Execucao.Application.Diagnosticos;

public sealed class IniciarDiagnosticoCommandValidator : AbstractValidator<IniciarDiagnosticoCommand>
{
    public IniciarDiagnosticoCommandValidator()
    {
        RuleFor(command => command.OrdemServicoId).GreaterThan(0);
        RuleFor(command => command.VeiculoId).GreaterThan(0);
        RuleFor(command => command.DescricaoSolicitacao).NotEmpty().MaximumLength(1000);
        RuleFor(command => command.ChaveIdempotencia).NotEmpty().MaximumLength(100);
        RuleFor(command => command.ServicoIdsSolicitados).NotEmpty();
        RuleForEach(command => command.ServicoIdsSolicitados).GreaterThan(0);
        RuleFor(command => command.PecaIdsSolicitadas).NotNull();
        RuleForEach(command => command.PecaIdsSolicitadas).GreaterThan(0);
    }
}
