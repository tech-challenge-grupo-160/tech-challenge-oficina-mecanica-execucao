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

Em produção, o início é um comando assíncrono enviado pelo OS Service à fila SQS
`execucao-iniciar-diagnostico`. O contrato versionado contém `messageId`,
`correlationId`, `ordemServicoId`, `veiculoId`, descrição e IDs solicitados. A
Execução registra a mensagem na Inbox, cria o diagnóstico e persiste o evento
`DiagnosticoIniciado` na Outbox na mesma transação. O evento é publicado no SNS
`oficina-eventos`; cada fila possui DLQ com três tentativas.

O endpoint abaixo permanece somente como apoio em `Development` e
`IntegrationTesting`; não é exposto em produção.

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

## Registrar resultado tecnico

`PUT /api/v1/diagnosticos/ordem-servico/{ordemServicoId}/resultado`

Registra a representacao atual do resultado tecnico sem concluir o diagnostico.
O status permanece `EmDiagnostico` e nenhuma operacao e realizada no OS Service
ou no Billing.

Request:

```json
{
  "parecerTecnico": "Pastilhas desgastadas e veiculo desalinhado.",
  "servicoIds": [1000],
  "pecas": [
    {
      "pecaId": 1000,
      "quantidade": 2
    }
  ]
}
```

Response `200 OK`:

```json
{
  "diagnosticoId": 1,
  "ordemServicoId": 3001,
  "parecerTecnico": "Pastilhas desgastadas e veiculo desalinhado.",
  "servicoIds": [1000],
  "pecas": [
    {
      "pecaId": 1000,
      "quantidade": 2
    }
  ],
  "atualizadoEm": "2026-11-01T22:00:00Z",
  "versao": 1
}
```

O `PUT` e idempotente: repetir o mesmo conteudo preserva `atualizadoEm` e
`versao`. Alterar o conteudo substitui o resultado anterior e incrementa a
versao. Atualizacoes concorrentes conflitantes retornam `409 Conflict`.

Servicos e pecas inexistentes ou inativos retornam `404 Not Found`. O resultado
exige ao menos um servico, enquanto a lista de pecas pode ser vazia.

## Consultar resultado tecnico

`GET /api/v1/diagnosticos/ordem-servico/{ordemServicoId}/resultado`

Retorna `200 OK` com o contrato acima. Retorna `404 Not Found` quando o
diagnostico nao existe ou ainda nao possui resultado registrado.

## Concluir diagnostico

`PATCH /api/v1/diagnosticos/ordem-servico/{ordemServicoId}/concluir`

Conclui um diagnostico que ja possui parecer e ao menos um servico no resultado
tecnico. A operacao altera o status para `Concluido` e grava o evento
`DiagnosticoConcluidoMessage` na Outbox na mesma transacao. O publicador envia o
evento ao topico SNS `oficina-eventos`; o OS Service o recebe pela sua fila SQS e
avanca a OS de `EmDiagnostico` para `AguardandoOrcamento`.

A repeticao retorna `200 OK` sem criar outro evento. Diagnostico inexistente
retorna `404 Not Found`; tentativa sem resultado tecnico retorna `400 Bad Request`.
