namespace Fiap.TechChallenge.OficinaMecanica.Execucao.Domain.Entities;

public sealed class Servico
{
    private Servico() { }

    public int Id { get; private set; }
    public string Nome { get; private set; } = null!;
    public string Descricao { get; private set; } = null!;
    public decimal Preco { get; private set; }
    public int TempoEstimado { get; private set; }
    public bool Ativo { get; private set; }

    public static Servico Criar(string nome, string descricao, decimal preco, int tempoEstimado)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome do servico e obrigatorio.", nameof(nome));
        if (string.IsNullOrWhiteSpace(descricao))
            throw new ArgumentException("Descricao do servico e obrigatoria.", nameof(descricao));
        if (preco < 0)
            throw new ArgumentException("Preco do servico nao pode ser negativo.", nameof(preco));
        if (tempoEstimado <= 0)
            throw new ArgumentException("Tempo estimado deve ser maior que zero.", nameof(tempoEstimado));

        return new Servico
        {
            Nome = nome.Trim(),
            Descricao = descricao.Trim(),
            Preco = preco,
            TempoEstimado = tempoEstimado,
            Ativo = true
        };
    }
}
