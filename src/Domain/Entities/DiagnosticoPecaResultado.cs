namespace Fiap.TechChallenge.OficinaMecanica.Execucao.Domain.Entities;

public sealed class DiagnosticoPecaResultado
{
    private DiagnosticoPecaResultado() { }

    public int DiagnosticoId { get; private set; }
    public int PecaId { get; private set; }
    public int Quantidade { get; private set; }

    internal static DiagnosticoPecaResultado Criar(int pecaId, int quantidade)
    {
        if (pecaId <= 0)
            throw new ArgumentException("PecaId deve ser maior que zero.", nameof(pecaId));
        if (quantidade <= 0)
            throw new ArgumentException("Quantidade deve ser maior que zero.", nameof(quantidade));

        return new DiagnosticoPecaResultado
        {
            PecaId = pecaId,
            Quantidade = quantidade
        };
    }
}
