# Execucao Service — Oficina Mecânica

[![Quality Gate](https://sonarcloud.io/api/project_badges/measure?project=tech-challenge-grupo-160_tech-challenge-oficina-mecanica-execucao&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=tech-challenge-grupo-160_tech-challenge-oficina-mecanica-execucao) [![Cobertura](https://sonarcloud.io/api/project_badges/measure?project=tech-challenge-grupo-160_tech-challenge-oficina-mecanica-execucao&metric=coverage)](https://sonarcloud.io/summary/overall?id=tech-challenge-grupo-160_tech-challenge-oficina-mecanica-execucao) [![Bugs](https://sonarcloud.io/api/project_badges/measure?project=tech-challenge-grupo-160_tech-challenge-oficina-mecanica-execucao&metric=bugs)](https://sonarcloud.io/summary/overall?id=tech-challenge-grupo-160_tech-challenge-oficina-mecanica-execucao) [![Vulnerabilidades](https://sonarcloud.io/api/project_badges/measure?project=tech-challenge-grupo-160_tech-challenge-oficina-mecanica-execucao&metric=vulnerabilities)](https://sonarcloud.io/summary/overall?id=tech-challenge-grupo-160_tech-challenge-oficina-mecanica-execucao)

Microsserviço da Fase 4 do Tech Challenge (FIAP SOAT, Grupo 160).

**Responsabilidade:** Diagnóstico e reparo, fila de execução, catálogo de serviços e peças, estoque, reserva de peças e pedidos de compra.

Faz parte da divisão definida na [ADR-0003](https://github.com/tech-challenge-grupo-160/tech-challenge-oficina-mecanica/blob/develop/docs/adrs/0003-divisao-em-microsservicos.md):

| Serviço | Repositório |
|---|---|
| OS Service | [tech-challenge-oficina-mecanica](https://github.com/tech-challenge-grupo-160/tech-challenge-oficina-mecanica) |
| Billing | [tech-challenge-oficina-mecanica-billing](https://github.com/tech-challenge-grupo-160/tech-challenge-oficina-mecanica-billing) |
| Execução | [tech-challenge-oficina-mecanica-execucao](https://github.com/tech-challenge-grupo-160/tech-challenge-oficina-mecanica-execucao) |

> **Estado:** catálogo próprio e jornada de diagnóstico implementados. O início chega por SQS; resultado e conclusão pertencem à Execução; os eventos de início e conclusão são publicados via SNS usando Outbox/Inbox e idempotência. Reparo entra nas próximas jornadas do [board da Fase 4](https://github.com/orgs/tech-challenge-grupo-160/projects/5).

## Stack

.NET 10 · ASP.NET Core · Clean Architecture (API → Application → Domain → Infrastructure) · MediatR com `ValidationBehavior` · FluentValidation · logs em JSON · Datadog .NET Tracer na imagem.

Banco: PostgreSQL em instância RDS própria, provisionado pela [F4-12](https://github.com/tech-challenge-grupo-160/tech-challenge-oficina-mecanica/issues/143). A integração assíncrona usa SNS/SQS, Outbox e Inbox; no Docker local, esses recursos são fornecidos pelo LocalStack do OS Service.

## Estrutura

```
src/
  API/              controllers, bootstrap, filtros, ProblemDetails
  Application/      comandos, consultas e handlers (MediatR), validação
  Domain/           entidades e regras de negócio
  Infrastructure/   persistência, mensageria, health checks, logs
  Shared/           código compartilhado entre camadas (LogTemplate)
tests/
  Fiap.TechChallenge.OficinaMecanica.Execucao.Test.UnitTests/
  Fiap.TechChallenge.OficinaMecanica.Execucao.Test.IntegrationTests/
docker/backend/     Dockerfile (desenvolvimento, build e runtime com o tracer)
k8s/                Deployment, Service, HPA e ConfigMap (Kustomize)
```

## Como rodar

```bash
dotnet run --project src/API/Fiap.TechChallenge.OficinaMecanica.Execucao.Api.csproj
```

Ou com Docker:

Crie um arquivo `.env` local (ignorado pelo Git) com a senha do PostgreSQL e a
connection string utilizada pela API:

```dotenv
POSTGRES_PASSWORD=<senha-local>
API_CONNECTION_STRING=Host=postgres;Database=oficina_execucao;Username=postgres;Password=<senha-local>
```

```bash
docker compose up --build
```

A API sobe em `http://localhost:8082`, com Swagger em `/swagger` no ambiente de desenvolvimento.

| Endpoint | Para quê |
|---|---|
| `/health/live` | O processo está de pé (liveness do Kubernetes) |
| `/health/ready` | O serviço e suas dependências estão prontos (readiness) |
| `/health` | Relatório completo em JSON |

O PostgreSQL da Execucao fica exposto em `localhost:5433` e usa o database `oficina_execucao`. As migrations e o seed inicial sao aplicados na inicializacao da API.

O endpoint interno `POST /api/v1/catalogo/itens/consultar` fornece snapshots comerciais ao OS Service sem expor estoque. Consulte [os contratos internos](docs/API_CONTRACTS.md).

O seed inicial disponibiliza IDs estaveis de `1000` a `1007`:

| ID | Servico | ID | Peca |
|---:|---|---:|---|
| 1000 | Alinhamento | 1000 | Pastilha de Freio |
| 1001 | Troca de Oleo | 1001 | Oleo de Motor 5W30 - 5L |
| 1002 | Balanceamento | 1002 | Filtro de Oleo |
| 1003 | Revisao do Sistema de Freios | 1003 | Disco de Freio |
| 1004 | Diagnostico Eletronico | 1004 | Correia Dentada |
| 1005 | Troca da Correia Dentada | 1005 | Vela de Ignicao |
| 1006 | Higienizacao do Ar-Condicionado | 1006 | Filtro de Ar-Condicionado |
| 1007 | Revisao Preventiva | 1007 | Bateria 60Ah |

## Testes

```bash
dotnet test Fiap.TechChallenge.OficinaMecanica.Execucao.slnx --collect:"XPlat Code Coverage" --settings coverlet.runsettings
```

A meta da fase é cobertura mínima de 80%, verificada no SonarCloud ([F4-09](https://github.com/tech-challenge-grupo-160/tech-challenge-oficina-mecanica/issues/140)).

## Fluxo de branches

`feature/*` → `develop` → `homolog` → `main`, com Conventional Commits. Detalhes no [CONTRIBUTING do repositório principal](https://github.com/tech-challenge-grupo-160/tech-challenge-oficina-mecanica/blob/develop/CONTRIBUTING.md).
