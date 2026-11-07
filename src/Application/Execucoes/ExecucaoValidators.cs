using FluentValidation;

namespace Fiap.TechChallenge.OficinaMecanica.Execucao.Application.Execucoes;

public sealed class IniciarExecucaoCommandValidator : AbstractValidator<IniciarExecucaoCommand>
{
    public IniciarExecucaoCommandValidator()
    {
        RuleFor(item => item.CorrelationId).NotEmpty();
        RuleFor(item => item.OrdemServicoId).GreaterThan(0);
        RuleFor(item => item.DiagnosticoId).GreaterThan(0);
        RuleFor(item => item.OrcamentoId).NotEmpty();
    }
}

public sealed class ObterExecucaoPorOrdemQueryValidator : AbstractValidator<ObterExecucaoPorOrdemQuery>
{
    public ObterExecucaoPorOrdemQueryValidator() =>
        RuleFor(item => item.OrdemServicoId).GreaterThan(0);
}

public sealed class FinalizarExecucaoCommandValidator : AbstractValidator<FinalizarExecucaoCommand>
{
    public FinalizarExecucaoCommandValidator() =>
        RuleFor(item => item.OrdemServicoId).GreaterThan(0);
}
