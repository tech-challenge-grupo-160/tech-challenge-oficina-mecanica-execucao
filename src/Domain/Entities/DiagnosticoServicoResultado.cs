namespace Fiap.TechChallenge.OficinaMecanica.Execucao.Domain.Entities;

public sealed class DiagnosticoServicoResultado
{
    private DiagnosticoServicoResultado() { }

    public int DiagnosticoId { get; private set; }
    public int ServicoId { get; private set; }

    internal static DiagnosticoServicoResultado Criar(int servicoId)
    {
        if (servicoId <= 0)
            throw new ArgumentException("ServicoId deve ser maior que zero.", nameof(servicoId));

        return new DiagnosticoServicoResultado { ServicoId = servicoId };
    }
}
