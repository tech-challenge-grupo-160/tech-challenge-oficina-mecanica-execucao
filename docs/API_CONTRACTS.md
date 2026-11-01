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

## Iniciar diagnostico

`POST /api/v1/diagnosticos`

O cabecalho `Idempotency-Key` e obrigatorio. O OS Service usa a chave estavel
`iniciar-diagnostico:{ordemServicoId}`.

Request:

```json
{
  "ordemServicoId": 3001,
  "veiculoId": 1001,
  "descricaoSolicitacao": "Ruido durante a frenagem",
  "servicoIdsSolicitados": [1000],
  "pecaIdsSolicitadas": [1000]
}
```

A primeira chamada retorna `201 Created`. Repetir a mesma chave e o mesmo payload
retorna `200 OK` com o diagnostico existente. Reutilizar a chave ou a ordem com
dados diferentes retorna `409 Conflict`.

Response:

```json
{
  "id": 1,
  "ordemServicoId": 3001,
  "veiculoId": 1001,
  "descricaoSolicitacao": "Ruido durante a frenagem",
  "servicoIdsSolicitados": [1000],
  "pecaIdsSolicitadas": [1000],
  "status": "EmDiagnostico",
  "iniciadoEm": "2026-11-01T21:30:00Z"
}
```

O diagnostico pode ser consultado por
`GET /api/v1/diagnosticos/ordem-servico/{ordemServicoId}`.
