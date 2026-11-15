# TechChallenge.Estoque — Microsserviço de Estoque

> Fase 4 — FIAP SOAT · Arquitetura de Microsserviços com Saga Pattern

Microsserviço responsável pelo **controle de estoque de peças**, reservando itens quando uma OS é aberta e liberando-os caso ela seja cancelada. Participa da Saga coreografada como serviço de suporte ao fluxo principal.

---

## 🔗 Repositórios do TechChallenger

| Serviço | Repositório | Responsabilidade |
|---------|-------------|------------------|
| OS Service | [TechChallenge.OS](https://github.com/TechChallenge01/TechChallenge.OS) | Ciclo de vida das Ordens de Serviço |
| Billing Service | [TechChallenge.Billing](https://github.com/TechChallenge01/TechChallenge.Billing) | Orçamentos e pagamentos (Mercado Pago) |
| Cadastro Service | [TechChallenge.Cadastro](https://github.com/TechChallenge01/TechChallenge.Cadastro) | Clientes, produtos e categorias |
| **Estoque Service** ← *este* | [TechChallenge.Estoque](https://github.com/TechChallenge01/TechChallenge.Estoque) | Reserva e liberação de peças |
| Execução Service | [TechChallenge.Execucao](https://github.com/TechChallenge01/TechChallenge.Execucao) | Fila de execução e reparos |
| BFF | [TechChallenge.bff](https://github.com/TechChallenge01/TechChallenge.bff) | Gateway + Frontend React |

---

## 🏗️ Arquitetura deste repositório

```mermaid
flowchart TD
    Client([Cliente / BFF]) -->|HTTP| API[API Layer\nMinimal API .NET 10]
    API --> App[Application Layer\nControllers · UseCases]
    App --> Domain[Domain Layer\nItemEstoque · Regras]
    App --> Infra[Infra Layer\nMongoDB · RabbitMQ]
    RMQ[(RabbitMQ\ntech.saga)] -->|os.aberta / os.cancelada| Infra
    Infra -->|estoque.reservado\nestoque.insuficiente\nestoque.liberado| RMQ
    Infra --> DB[(MongoDB\nestoque)]
```

**Camadas (Clean Architecture):**

| Camada | Conteúdo |
|--------|----------|
| `Domain` | `ItemEstoque`, regras de reserva e liberação |
| `Shared` | DTOs, interfaces compartilhadas |
| `Application` | Controllers, UseCases (reservar, liberar, ajustar) |
| `Infra` | MongoDB, RabbitMQ consumer/publisher |
| `API` | Endpoints Minimal API, Program.cs |

**Deploy:** AWS ECS Fargate · cluster `techchallenge` · região `us-east-1`

---

## 🎯 Objetivo da Fase 4

- Refatorar a aplicação monolítica em microsserviços independentes
- Implementar o **Saga Pattern Coreografado** via RabbitMQ para consistência distribuída
- Garantir rollback de estoque automático quando uma OS é cancelada
- Pipeline CI/CD independente com testes automatizados e deploy no ECS Fargate
- Monitoramento e observabilidade com **CloudWatch** e **Datadog**

---

## 🔄 Saga Pattern — Papel do Estoque

O Estoque atua como **guardião de recursos**: reserva peças quando a OS é aberta e as libera automaticamente se a OS for cancelada em qualquer etapa da Saga.

**Exchange:** `tech.saga` (Topic, durable) · Broker: RabbitMQ 3.13 no EC2 `100.24.30.116`

### Eventos consumidos por este serviço

| Routing Key | Fila | Ação |
|-------------|------|------|
| `os.aberta` | `estoque.os.aberta` | Reservar peças necessárias |
| `os.cancelada` | `estoque.os.cancelada` | Liberar peças reservadas (compensação) |

### Eventos publicados por este serviço

| Routing Key | Quando | Payload |
|-------------|--------|---------|
| `estoque.reservado` | Peças reservadas com sucesso | `{ OrdemServicoId, Itens }` |
| `estoque.insuficiente` | Estoque insuficiente para a OS | `{ OrdemServicoId, ProdutoId }` |
| `estoque.liberado` | Peças liberadas (compensação de cancelamento) | `{ OrdemServicoId, Itens }` |

---

## ⚙️ Pré-requisitos

**Execução local:**
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- MongoDB 7.0 (via Docker ou Atlas)
- RabbitMQ 3.13 (via Docker)

**Infraestrutura AWS:**
- AWS CLI configurado (`aws configure`)
- Terraform ≥ 1.9
- Acesso ao cluster ECS `techchallenge` em `us-east-1`

---

## 🚀 Como Executar

### Local com Docker Compose

```bash
git clone https://github.com/TechChallenge01/TechChallenge.Estoque.git
cd TechChallenge.Estoque
docker compose up -d
```

Acesse: `http://localhost:5002/scalar` (Scalar API Docs)

### Testes

```bash
dotnet test tests/UnitTests
```

---

## 🔐 Variáveis de Ambiente

| Variável | Descrição | Exemplo |
|----------|-----------|---------|
| `MongoDB__ConnectionString` | URI do MongoDB | `mongodb://localhost:27017` |
| `MongoDB__DatabaseName` | Nome do banco | `estoque` |
| `RabbitMQ__Host` | Host do RabbitMQ | `localhost` |
| `RabbitMQ__Username` | Usuário RabbitMQ | `guest` |
| `RabbitMQ__Password` | Senha RabbitMQ | `guest` |
| `Jwt__Key` | Chave JWT (mín. 32 chars) | — |
| `Jwt__Issuer` | Issuer JWT | `TechChallenger` |
| `Jwt__Audience` | Audience JWT | `TechChallenger` |

---

## 📡 Endpoints da API

Todos os endpoints (exceto `/health`) requerem `Authorization: Bearer <token>`.

| Método | Rota | Descrição |
|--------|------|-----------|
| `GET` | `/health` | Health check |
| `GET` | `/estoque/{produtoId}` | Consultar estoque de um produto |
| `POST` | `/estoque` | Adicionar item ao estoque |
| `PUT` | `/estoque/{produtoId}/ajustar` | Ajustar quantidade em estoque |

**Exemplo — Adicionar item:**
```json
POST /estoque
{
  "produtoId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "quantidade": 100,
  "unidade": "un"
}
```

**Exemplo — Ajustar estoque:**
```json
PUT /estoque/3fa85f64-5717-4562-b3fc-2c963f66afa6/ajustar
{
  "novaQuantidade": 50
}
```

---

## 🏭 Infraestrutura

**Deploy:** AWS ECS Fargate · cluster `techchallenge` · `us-east-1`

Infraestrutura como código em `infra/` (Terraform):

| Arquivo | Recurso |
|---------|---------|
| `ecs.tf` | ECS Service + Task Definition |
| `ecr.tf` | Repositório ECR para a imagem Docker |
| `alb.tf` | Application Load Balancer |
| `cloudwatch.tf` | Log groups `/ecs/estoque-service` |
| `data-sources.tf` | Data sources (VPC, subnets, cluster ECS) |

```bash
cd infra
terraform init
terraform plan
terraform apply
```

---

## 📦 Kubernetes

Manifestos em `k8s/`:

| Arquivo | Recurso |
|---------|---------|
| `deployment.yaml` | 2 réplicas, liveness/readiness probes |
| `service.yaml` | ClusterIP porta 80 |
| `hpa.yaml` | Auto-scaling 2–5 pods (70% CPU) |

---

## 🔁 CI/CD

| Workflow | Trigger | Jobs |
|----------|---------|------|
| `deploy.yml` | push `main` (src/) | Build → Push ECR → Deploy ECS |
| `quality.yml` | push/PR `main` | Testes unitários → SonarCloud |
| `infra.yml` | push `main` (infra/) | Terraform plan/apply |
| `k8s-deploy.yml` | manual (`workflow_dispatch`) | Deploy EKS |

---

## 🧪 Testes

```bash
dotnet test tests/UnitTests

# Com cobertura
dotnet test tests/UnitTests --collect:"XPlat Code Coverage" --results-directory coverage/
```

- **Cobertura mínima:** 80% (validada pelo SonarCloud no CI)
- **Relatório:** artefato `coverage-report-estoque` em cada execução do `quality.yml`

---

## 📚 Documentação da API

Scalar UI disponível em:

- **Local:** `http://localhost:5002/scalar`
- **Produção:** via BFF ALB

---

## 📬 Collection Postman

Collection completa (todos os serviços) disponível em:

📂 [`TechChallenge.bff/docs/TechChallenger-Postman-Collection.json`](https://github.com/TechChallenge01/TechChallenge.bff/blob/main/docs/TechChallenger-Postman-Collection.json)

---

## 🗂️ Estrutura do Projeto

```
TechChallenge.Estoque/
├── src/
│   ├── Domain/          # ItemEstoque, regras de estoque
│   ├── Shared/          # DTOs, interfaces compartilhadas
│   ├── Application/     # Controllers, UseCases
│   ├── Infra/           # MongoDB, RabbitMQ consumer/publisher
│   └── API/             # Endpoints, Program.cs
├── tests/
│   └── UnitTests/       # Testes unitários (xUnit)
├── k8s/                 # Manifestos Kubernetes
├── infra/               # Terraform (ECS, ECR, ALB, CloudWatch)
└── .github/workflows/   # CI/CD pipelines
```
