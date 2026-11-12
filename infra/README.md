# Mensageria da Execucao

Este diretorio provisiona o topico SNS publicado pela Execucao, uma fila SQS por
comando que ela consome, uma DLQ por fila e a configuracao no AWS Secrets
Manager. E a mesma topologia do LocalStack (`docker/localstack/init-aws.sh` do
OS Service), descrita no adendo de 11/11 a ADR-0005: uma fila por tipo de
mensagem, sem roteamento pelo campo `type`.

| Fila | Comando | Chave que o codigo le |
|---|---|---|
| `...-execucao-iniciar-diagnostico` | `IniciarDiagnosticoMessage` | `InicioDiagnosticoQueueUrl` |
| `...-execucao-reservar-pecas` | `ReservarPecasMessage` | `ReservaPecasQueueUrl` |
| `...-execucao-iniciar-reparo` | `IniciarExecucaoMessage` | `InicioExecucaoQueueUrl` |

Os comandos chegam por envio direto do OS Service (`SendMessage`), sem topico
nem assinatura. O topico `tc-grupo160-<ambiente>-execucao-events` recebe os eventos
(`DiagnosticoIniciado`, `DiagnosticoConcluido`, `PecasReservadas`,
`EstoqueInsuficiente`, `ExecucaoFinalizada`); o publicador da Outbox grava o tipo no atributo `tipo`
e quem filtra sao as filas do OS Service.

O topico usa a chave gerenciada pela AWS para SNS. Filas e DLQs usam
criptografia gerenciada pelo SQS, retencao de 4 e 14 dias e redrive apos 5
recebimentos.

O segredo `tc-grupo160/<ambiente>/execucao/messaging` tem exatamente as chaves
`Messaging__*` que o codigo le, inclusive `Messaging__Enabled`. O workflow de
deploy as injeta pelo parametro `segredos` e o `sobe-tudo.sh` copia todas para
o Secret do pod. O catalogo esta em
[docs/mensageria/catalogo.md](https://github.com/tech-challenge-grupo-160/tech-challenge-oficina-mecanica/blob/develop/docs/mensageria/catalogo.md).

## Aplicacao

```powershell
terraform init -reconfigure `
  -backend-config="bucket=tc-grupo160-tfstate-<id-da-conta>" `
  -backend-config="key=dev/messaging-execucao.tfstate" `
  -backend-config="region=us-east-1" `
  -backend-config="dynamodb_table=tc-grupo160-tflock"
terraform plan -var="environment=dev"
```

No primeiro provisionamento, crie primeiro somente `aws_sns_topic.events` em
cada servico. Com os tres topicos presentes, aplique o Terraform completo nos
consumidores para criar filas, assinaturas, filtros e segredos. Use a mesma
conta, regiao, ambiente e prefixo nos tres repositorios. O workflow
`terraform-messaging.yml` planeja PRs e aplica em push nas branches de ambiente;
sem push, este commit não dispara apply.

O roteiro PowerShell de provisionamento e validação no AWS Academy, incluindo
filtros, redrive e criptografia, está no
[catálogo de mensageria](https://github.com/tech-challenge-grupo-160/tech-challenge-oficina-mecanica/blob/develop/docs/mensageria/catalogo.md#teste-direto-na-aws-academy-learner-lab).
O acesso de pods EKS ao SNS/SQS não é validado apenas por criar recursos e
ainda requer o spike F4-05.
