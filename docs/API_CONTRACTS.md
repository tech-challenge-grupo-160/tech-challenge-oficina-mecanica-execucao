# Contratos internos da Execucao

## Consultar itens do catalogo

`POST /api/v1/catalogo/itens/consultar`

Este endpoint e usado pelo OS Service durante a abertura da ordem. Ele valida IDs e fornece snapshots comerciais sem expor saldo de estoque.

Request:

```json
{
  "servicoIds": [1000],
  "pecaIds": [1000]
}
```

Response `200 OK`:

```json
{
  "servicos": [
    {
      "id": 1000,
      "nome": "Alinhamento",
      "preco": 150.00,
      "tempoEstimado": 30
    }
  ],
  "pecas": [
    {
      "id": 1000,
      "nome": "Pastilha de Freio",
      "preco": 45.00
    }
  ],
  "servicoIdsNaoEncontrados": [],
  "pecaIdsNaoEncontrados": []
}
```

IDs inexistentes ou inativos aparecem nas listas `servicoIdsNaoEncontrados` e `pecaIdsNaoEncontrados`. Uma consulta sem IDs ou com IDs menores ou iguais a zero retorna `400 Bad Request` em `application/problem+json`.

O endpoint nao reserva pecas, nao valida disponibilidade e nao baixa estoque. Essas operacoes pertencem a comandos posteriores da Saga.
