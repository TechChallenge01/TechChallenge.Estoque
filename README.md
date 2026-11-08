# TechChallenge.Estoque — Microsserviço de Estoque

Microsserviço responsável pelo controle de estoque e reservas de produtos, parte do sistema TechChallenger (oficina mecânica).

## Responsabilidades

- Controle de quantidade disponível por produto
- Reserva de itens para uma OS (sem dedução definitiva)
- Liberação de reservas em caso de cancelamento da OS
- Baixa definitiva após execução finalizada

## Arquitetura

Clean Architecture com 5 camadas: **Domain**, **Shared**, **Application**, **Infra**, **API**.

## Banco de Dados

- **MongoDB** — banco de dados NoSQL
- Coleção: `itens_estoque`
- Cada documento rastreia `quantidadeDisponivel` e `quantidadeReservada` por `produtoId`

## Padrão Saga — Coreografado via RabbitMQ

### Por que Coreografia?

A coreografia permite que o Estoque reaja a eventos do fluxo sem depender de um orquestrador. A reserva atômica (sucesso → `estoque.reservado`, falha → `estoque.reserva.falhou`) garante consistência eventual sem locks distribuídos.

### Eventos publicados

| Exchange | Routing Key | Quando |
|----------|-------------|--------|
| `tech.saga` | `estoque.reservado` | Reserva realizada com sucesso |
| `tech.saga` | `estoque.reserva.falhou` | Estoque insuficiente para a OS |

### Eventos consumidos

| Exchange | Routing Key | Ação |
|----------|-------------|------|
| `tech.saga` | `os.aberta` | Tenta reservar materiais necessários |
| `tech.saga` | `os.cancelada` | Libera reserva associada à OS |
| `tech.saga` | `execucao.finalizada` | Confirma baixa definitiva do estoque |

### Compensação (Rollback)

- `os.cancelada` → `ItemEstoque.Liberar()` devolve quantidade reservada ao disponível
- Falha na reserva → publica `estoque.reserva.falhou`; OS Service inicia fluxo de cancelamento
- Dead Letter Queue (`estoque.saga.dlq`)

## Executando localmente

```bash
# Pré-requisitos: Docker, .NET 10 SDK
docker compose up -d
dotnet run --project src/API
```

## Variáveis de ambiente

| Variável | Descrição |
|----------|-----------|
| `ConnectionStrings__MongoDB` | URI do MongoDB |
| `MongoDB__DatabaseName` | Nome do banco (default: `techchallenge_estoque`) |
| `RabbitMQ__Host` | Host do RabbitMQ |
| `RabbitMQ__Username` | Usuário RabbitMQ |
| `RabbitMQ__Password` | Senha RabbitMQ |
| `Jwt__Key` | Chave JWT |

## Endpoints principais

| Método | Rota | Descrição |
|--------|------|-----------|
| `GET` | `/estoque` | Listar itens em estoque |
| `GET` | `/estoque/{produtoId}` | Consultar estoque de produto |
| `POST` | `/estoque` | Registrar item de estoque |
| `PUT` | `/estoque/{produtoId}/ajustar` | Ajustar quantidade disponível |
| `POST` | `/estoque/{produtoId}/reservar` | Reservar quantidade manualmente |
| `POST` | `/estoque/{produtoId}/liberar` | Liberar reserva manualmente |

## Kubernetes

Manifestos em `k8s/`: `deployment.yaml`, `service.yaml`, `hpa.yaml`.

## CI/CD

| Workflow | Trigger | Descrição |
|----------|---------|-----------|
| `deploy.yml` | push main | Build, ECR, ECS deploy |
| `infra.yml` | push main (infra/) | Terraform apply |
| `quality.yml` | push/PR main | Testes + SonarCloud |
| `k8s-deploy.yml` | manual | Deploy para EKS |

## Testes

```bash
dotnet test tests/UnitTests
```

Cobertura mínima: **80%** — validada pelo SonarCloud.

**Configuração SonarCloud**: Secret `SONAR_TOKEN`, Variables `SONAR_PROJECT_KEY` e `SONAR_ORGANIZATION`.
