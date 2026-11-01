namespace Fiap.TechChallenge.OficinaMecanica.Execucao.Domain.Entities;

public sealed class Peca
{
    private Peca() { }

    public int Id { get; private set; }
    public string Nome { get; private set; } = null!;
    public string Marca { get; private set; } = null!;
    public string Modelo { get; private set; } = null!;
    public decimal Preco { get; private set; }
    public int QuantidadeEstoque { get; private set; }
    public bool Ativa { get; private set; }

    public static Peca Criar(
        string nome, string marca, string modelo, decimal preco, int quantidadeEstoque)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome da peca e obrigatorio.", nameof(nome));
        if (string.IsNullOrWhiteSpace(marca))
            throw new ArgumentException("Marca da peca e obrigatoria.", nameof(marca));
        if (string.IsNullOrWhiteSpace(modelo))
            throw new ArgumentException("Modelo da peca e obrigatorio.", nameof(modelo));
        if (preco < 0)
            throw new ArgumentException("Preco da peca nao pode ser negativo.", nameof(preco));
        if (quantidadeEstoque < 0)
            throw new ArgumentException("Quantidade em estoque nao pode ser negativa.", nameof(quantidadeEstoque));

        return new Peca
        {
            Nome = nome.Trim(),
            Marca = marca.Trim(),
            Modelo = modelo.Trim(),
            Preco = preco,
            QuantidadeEstoque = quantidadeEstoque,
            Ativa = true
        };
    }
}
