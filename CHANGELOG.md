# Changelog

Todas as mudancas notaveis do servico serao documentadas neste arquivo.

O formato segue [Keep a Changelog](https://keepachangelog.com/pt-BR/1.1.0/).

## [Unreleased]

## [4.0.0] - 2026-11-15

### Changed
- Uma fila por comando, com as chaves de configuracao que o codigo le (adendo a ADR-0005)
- Request de memoria de 128Mi para 256Mi e Service como NodePort 30082 para o ALB (F4-14, F4-38)
- Campo `status` do log de requisicao HTTP renomeado para `statusHttp`, para o Datadog nao ler o codigo HTTP como nivel do log (F4-32)
- CI/CD pelo pipeline reutilizavel `servico-dotnet.yml@v1` do `tech-challenge-pipelines`: build, testes com cobertura e imagem em todo PR; deploy no EKS no push da `homolog` e da `main`, dev pelo disparo manual (F4-08)

### Added
- Catalogo de servicos e pecas consultado pelo OS Service na abertura da OS (F4-16)
- Diagnostico com inicio idempotente, resultado e conclusao publicando eventos da Saga (F4-22)
- Reserva idempotente de pecas, fila de execucao e finalizacao do reparo comunicada ao OS Service (F4-22, F4-24)
- Reposicao de estoque que retoma a Saga de uma OS aguardando pecas
- PostgreSQL proprio no RDS, com a connection string montada no deploy (F4-12)
- Trace do Datadog propagado pela Outbox e pelo SNS/SQS (F4-31)
- Logs estruturados para seguir uma OS entre os servicos (F4-32): todo log JSON sai com `correlationId`, `ordemServicoId`, `trace_id`/`span_id` e bloco `dd` (`dd.trace_id`, `dd.span_id`) para o Datadog ligar log e trace; escopo aberto nas requisicoes HTTP, nos consumidores SQS e na Outbox; `DD_LOGS_INJECTION=true` na imagem; campos `level` e `message` reconhecidos pelo Datadog; mascara de CNPJ alem do CPF
- Pod com `tags.datadoghq.com/service` e `tags.datadoghq.com/env` e anotacao de logs `source: csharp`, para logs e traces do servico se ligarem no Datadog (F4-32)
- Esqueleto do servico: Clean Architecture, MediatR com validacao, health checks, logs em JSON, Dockerfile com o Datadog Tracer, manifests Kubernetes e CI de build e testes (F4-07)
- Terraform por serviço para o tópico SNS da Execução, filas de eventos/comandos, DLQs, filtros, criptografia, retenção e configuração no Secrets Manager (F4-11)
- Workflow independente para validar, planejar e aplicar a infraestrutura de mensageria por ambiente

### Fixed
- Achados do SonarCloud na primeira analise da `main`: `cancellationToken` repassado no `ValidationBehavior` e no health check, download do tracer do Datadog so por HTTPS, `DD_ENV` padrao sem `Development` e porta HTTP por `ASPNETCORE_HTTP_PORTS` no lugar de URL em texto claro
- Credenciais removidas da configuracao versionada
