namespace Fiap.TechChallenge.OficinaMecanica.Execucao.Domain.Entities;

public sealed class ReservaEstoque
{
    private readonly List<ReservaEstoqueItem> _itens = [];

    private ReservaEstoque() { }

    public Guid Id { get; private set; }
    public Guid CorrelationId { get; private set; }
    public Guid EventoResultadoId { get; private set; }
    public int OrdemServicoId { get; private set; }
    public Guid OrcamentoId { get; private set; }
    public string Status { get; private set; } = null!;
    public DateTimeOffset CriadaEm { get; private set; }
    public IReadOnlyCollection<ReservaEstoqueItem> Itens => _itens;

    public static ReservaEstoque Criar(
        Guid correlationId,
        int ordemServicoId,
        Guid orcamentoId,
        IEnumerable<ReservaEstoqueItem> itens,
        DateTimeOffset criadaEm)
    {
        if (correlationId == Guid.Empty)
            throw new ArgumentException("CorrelationId e obrigatorio.", nameof(correlationId));
        if (ordemServicoId <= 0)
            throw new ArgumentException("OrdemServicoId deve ser maior que zero.", nameof(ordemServicoId));
        if (orcamentoId == Guid.Empty)
            throw new ArgumentException("OrcamentoId e obrigatorio.", nameof(orcamentoId));

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
            OrcamentoId = orcamentoId,
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
        Guid orcamentoId,
        IReadOnlyCollection<(int PecaId, int Quantidade)> itens) =>
        CorrelationId == correlationId &&
        OrcamentoId == orcamentoId &&
        _itens.Count == itens.Count &&
        _itens.All(item => itens.Any(solicitado =>
            solicitado.PecaId == item.PecaId &&
            solicitado.Quantidade == item.QuantidadeSolicitada));

    public bool TentarCompletarItem(int pecaId, Peca peca)
    {
        if (pecaId <= 0)
            throw new ArgumentException("PecaId deve ser maior que zero.", nameof(pecaId));
        if (Status != "EstoqueInsuficiente")
            return false;

        var item = _itens.SingleOrDefault(item => item.PecaId == pecaId);
        if (item is null || item.QuantidadeFaltante == 0)
            return false;

        var quantidadeReservada = peca.ReservarSeDisponivel(item.QuantidadeFaltante);
        if (quantidadeReservada == 0)
            return false;

        item.AdicionarReserva(quantidadeReservada);
        if (_itens.All(itemReserva => itemReserva.QuantidadeFaltante == 0))
        {
            Status = "Reservada";
            EventoResultadoId = Guid.NewGuid();
        }

        return true;
    }
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

    internal void AdicionarReserva(int quantidade)
    {
        if (quantidade <= 0 || quantidade > QuantidadeFaltante)
            throw new ArgumentException("Quantidade adicional de reserva invalida.", nameof(quantidade));

        QuantidadeReservada += quantidade;
    }
}
