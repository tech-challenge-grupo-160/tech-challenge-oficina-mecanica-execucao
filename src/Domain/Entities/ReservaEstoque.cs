namespace Fiap.TechChallenge.OficinaMecanica.Execucao.Domain.Entities;

public sealed class ReservaEstoque
{
    private readonly List<ReservaEstoqueItem> _itens = [];

    private ReservaEstoque() { }

    public Guid Id { get; private set; }
    public Guid CorrelationId { get; private set; }
    public Guid EventoResultadoId { get; private set; }
    public int OrdemServicoId { get; private set; }
    public string Status { get; private set; } = null!;
    public DateTimeOffset CriadaEm { get; private set; }
    public IReadOnlyCollection<ReservaEstoqueItem> Itens => _itens;

    public static ReservaEstoque Criar(
        Guid correlationId,
        int ordemServicoId,
        IEnumerable<ReservaEstoqueItem> itens,
        DateTimeOffset criadaEm)
    {
        if (correlationId == Guid.Empty)
            throw new ArgumentException("CorrelationId e obrigatorio.", nameof(correlationId));
        if (ordemServicoId <= 0)
            throw new ArgumentException("OrdemServicoId deve ser maior que zero.", nameof(ordemServicoId));

        var itensMaterializados = itens.OrderBy(item => item.PecaId).ToArray();
        if (itensMaterializados.Length == 0)
            throw new ArgumentException("Ao menos uma peca e obrigatoria para reservar estoque.", nameof(itens));
        if (itensMaterializados.Select(item => item.PecaId).Distinct().Count() != itensMaterializados.Length)
            throw new ArgumentException("Nao e permitido repetir pecas na reserva.", nameof(itens));

        var reserva = new ReservaEstoque
        {
            Id = Guid.NewGuid(),
            CorrelationId = correlationId,
            EventoResultadoId = Guid.NewGuid(),
            OrdemServicoId = ordemServicoId,
            Status = itensMaterializados.All(item => item.QuantidadeFaltante == 0)
                ? "Reservada"
                : "EstoqueInsuficiente",
            CriadaEm = criadaEm
        };
        reserva._itens.AddRange(itensMaterializados);
        return reserva;
    }

    public bool Corresponde(
        Guid correlationId,
        IReadOnlyCollection<(int PecaId, int Quantidade)> itens) =>
        CorrelationId == correlationId &&
        _itens.Count == itens.Count &&
        _itens.All(item => itens.Any(solicitado =>
            solicitado.PecaId == item.PecaId &&
            solicitado.Quantidade == item.QuantidadeSolicitada));
}

public sealed class ReservaEstoqueItem
{
    private ReservaEstoqueItem() { }

    public Guid ReservaEstoqueId { get; private set; }
    public int PecaId { get; private set; }
    public int QuantidadeSolicitada { get; private set; }
    public int QuantidadeReservada { get; private set; }
    public int QuantidadeFaltante => QuantidadeSolicitada - QuantidadeReservada;

    public static ReservaEstoqueItem Criar(
        int pecaId,
        int quantidadeSolicitada,
        int quantidadeReservada)
    {
        if (pecaId <= 0)
            throw new ArgumentException("PecaId deve ser maior que zero.", nameof(pecaId));
        if (quantidadeSolicitada <= 0)
            throw new ArgumentException("Quantidade solicitada deve ser maior que zero.", nameof(quantidadeSolicitada));
        if (quantidadeReservada < 0 || quantidadeReservada > quantidadeSolicitada)
            throw new ArgumentException("Quantidade reservada invalida.", nameof(quantidadeReservada));

        return new ReservaEstoqueItem
        {
            PecaId = pecaId,
            QuantidadeSolicitada = quantidadeSolicitada,
            QuantidadeReservada = quantidadeReservada
        };
    }
}
