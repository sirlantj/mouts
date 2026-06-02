# Ambev Developer Evaluation - DeveloperStore

Sistema de gerenciamento de vendas com regras de desconto por quantidade, construido com .NET 8 (backend) e Angular 21 (frontend).

---

## Arquitetura

### Backend (.NET 8 / C#)

Segue **Clean Architecture** com separacao em camadas:

```
backend/
├── src/
│   ├── Ambev.DeveloperEvaluation.WebApi        # API REST, controllers, Swagger, middlewares
│   ├── Ambev.DeveloperEvaluation.Application   # Use cases (CQRS com MediatR), handlers, validators
│   ├── Ambev.DeveloperEvaluation.Domain        # Entidades, enums, regras de negocio, eventos, specifications
│   ├── Ambev.DeveloperEvaluation.ORM           # EF Core (PostgreSQL), MongoDB repos, Redis cache, migrations
│   ├── Ambev.DeveloperEvaluation.IoC           # Injecao de dependencia centralizada
│   └── Ambev.DeveloperEvaluation.Common        # Utilitarios compartilhados (JWT, hashing, validacao, health checks)
├── tests/
│   ├── Ambev.DeveloperEvaluation.Unit          # Testes unitarios
│   ├── Ambev.DeveloperEvaluation.Integration   # Testes de integracao
│   └── Ambev.DeveloperEvaluation.Functional    # Testes funcionais
├── docker-compose.yml                          # Orquestracao de todos os servicos
└── seed-data.sql                               # Script SQL com dados de teste
```

**Patterns utilizados:**
- **CQRS** - Commands e Queries separados via MediatR
- **Domain Events** - Eventos de dominio (SaleCreated, SaleModified, SaleCancelled, ItemCancelled) publicados via MediatR
- **Repository Pattern** - Abstracoes de repositorio no Domain, implementacoes no ORM
- **Specification Pattern** - Regras de negocio encapsuladas (ActiveUserSpecification, ActiveSaleSpecification)
- **Event Sourcing (leitura)** - MongoDB armazena eventos e read models das vendas
- **Cache-aside** - Redis como cache de leitura com invalidacao por prefixo

**Principais entidades:**
| Entidade | Descricao |
|----------|-----------|
| `User` | Usuarios com roles (Admin, Manager, Customer) e status (Active, Inactive, Suspended) |
| `Sale` | Vendas com numero, cliente, filial, status e itens |
| `SaleItem` | Itens da venda com calculo automatico de desconto por quantidade |

**Regras de desconto:**
| Quantidade | Desconto |
|-----------|----------|
| 1-3 itens | 0% |
| 4-9 itens | 10% |
| 10-20 itens | 20% |
| > 20 itens | Nao permitido |

**Endpoints da API:**

| Metodo | Rota | Descricao |
|--------|------|-----------|
| POST | `/api/auth` | Autenticacao (retorna JWT) |
| POST | `/api/users` | Criar usuario |
| GET | `/api/users/{id}` | Buscar usuario |
| DELETE | `/api/users/{id}` | Deletar usuario |
| POST | `/api/sales` | Criar venda |
| GET | `/api/sales` | Listar vendas (paginado, com filtros) |
| GET | `/api/sales/{id}` | Detalhe da venda |
| PUT | `/api/sales/{id}` | Atualizar venda |
| DELETE | `/api/sales/{id}` | Cancelar venda (soft delete) |
| PATCH | `/api/sales/{saleId}/items/{itemId}/cancel` | Cancelar item especifico |

**Filtros da listagem (GET /api/sales):**
- `_page`, `_size` - Paginacao
- `_order` - Ordenacao (ex: `saleDate desc`)
- `CustomerName`, `BranchName`, `Status` - Filtros por campo
- `StartDate`, `EndDate` - Filtro por periodo

---

### Frontend (Angular 21)

SPA com Angular 21, standalone components, signals para state management e TailwindCSS 4 para estilizacao.

```
frontend/
├── src/
│   ├── app/
│   │   ├── core/
│   │   │   ├── auth/
│   │   │   │   ├── auth.service.ts      # Autenticacao com signals (login, logout, token)
│   │   │   │   ├── auth.guard.ts        # Guard de rota (redireciona p/ login)
│   │   │   │   └── auth.interceptor.ts  # Interceptor HTTP (injeta JWT no header)
│   │   │   └── services/
│   │   │       └── sales.service.ts     # CRUD de vendas via HttpClient
│   │   ├── features/
│   │   │   ├── login/login.ts           # Tela de login
│   │   │   └── sales/
│   │   │       ├── sales-list/          # Listagem com paginacao e filtros
│   │   │       ├── sales-form/          # Formulario de criacao/edicao
│   │   │       └── sales-detail/        # Detalhe da venda com cancelamento de itens
│   │   └── shared/
│   │       ├── confirm-dialog/          # Dialog de confirmacao reutilizavel
│   │       └── pagination/              # Componente de paginacao
│   ├── environments/                    # Configuracoes por ambiente
│   └── styles.scss                      # Estilos globais + TailwindCSS
├── angular.json
├── Dockerfile                           # Build multi-stage (Node + Nginx)
└── nginx.conf                           # Proxy reverso p/ API
```

**Rotas:**
| Rota | Componente | Protegida |
|------|-----------|-----------|
| `/login` | LoginComponent | Nao |
| `/sales` | SalesListComponent | Sim |
| `/sales/new` | SalesFormComponent | Sim |
| `/sales/:id` | SalesDetailComponent | Sim |
| `/sales/:id/edit` | SalesFormComponent | Sim |

---

## Decisoes Tecnicas

### Backend

| Decisao | Justificativa |
|---------|--------------|
| **Clean Architecture** | Desacoplamento entre camadas; o Domain nao depende de infraestrutura |
| **CQRS com MediatR** | Separacao de responsabilidades entre leitura e escrita, facilita testes |
| **PostgreSQL (EF Core)** | Banco relacional para dados transacionais (Users, Sales, SaleItems) |
| **MongoDB** | Event store para auditoria e read models desnormalizados para consultas rapidas |
| **Redis** | Cache distribuido para respostas de listagem, com invalidacao por prefixo |
| **BCrypt** | Hash de senha com salt automatico, resistente a ataques de forca bruta |
| **JWT** | Autenticacao stateless, com role claims para autorizacao |
| **FluentValidation** | Validacao declarativa nos commands e requests, pipeline behavior no MediatR |
| **Domain Events** | Desacoplamento entre acao e side-effects (log, cache invalidation, event store) |
| **Soft Delete** | Vendas canceladas mantem historico em vez de serem removidas fisicamente |

### Frontend

| Decisao | Justificativa |
|---------|--------------|
| **Angular 21 Standalone** | Sem NgModules; imports diretos nos componentes, reduz boilerplate |
| **Signals** | State management reativo nativo do Angular, sem libs externas (NgRx) |
| **TailwindCSS 4** | Utility-first CSS, rapido para prototipar UI responsiva |
| **Lazy Loading** | Rotas com `loadComponent()` para code-splitting automatico |
| **HTTP Interceptor** | Injeta token JWT em todas as requests autenticadas de forma transparente |
| **Nginx** | Serve o build statico e faz proxy reverso para a API no Docker |

---

## Como Rodar

### Pre-requisitos

- Docker e Docker Compose
- (Opcional para dev local) .NET 8 SDK, Node.js 20+, npm

### Subir tudo com Docker (recomendado)

```bash
cd template/backend
docker-compose up --build -d
```

Isso sobe:
| Servico | Porta | Descricao |
|---------|-------|-----------|
| API (.NET) | `8080` | Swagger em http://localhost:8080/swagger |
| PostgreSQL | `5432` | Banco relacional |
| MongoDB | `27017` | Event store + read models |
| Redis | `6379` | Cache |
| Frontend (Angular) | `4200` | App em http://localhost:4200 |

### Carregar dados de teste

Sobe o `docker-compose --project-name ambev_eval up -d` e depois roda o script de seed:

```bash
cd template/backend
./seed-via-api.sh
```

O script:
1. Limpa `Sales`/`SaleItems` no Postgres e `sales_read`/`sale_events` no Mongo
2. Faz login como admin e usa o token JWT para criar as 10 vendas via `POST /api/sales` (popula Postgres + Mongo via domain events)
3. Cancela a `SALE-005` via `DELETE /api/sales/{id}`
4. Cancela um item da `SALE-006` via `PATCH /api/sales/{saleId}/items/{itemId}/cancel`

**Alternativa (so para popular Postgres, sem Mongo/eventos):**
```bash
docker exec -i ambev_developer_evaluation_database psql -U developer -d developer_evaluation < seed-data.sql
```
Use somente se quiser dados crus no Postgres — a API le do Mongo (CQRS), entao a listagem fica vazia se voce so usar o SQL.

### Credenciais de teste

| Email | Senha | Role | Status |
|-------|-------|------|--------|
| admin@ambev.com | Test@123 | Admin | Active |
| manager@ambev.com | Test@123 | Manager | Active |
| customer@ambev.com | Test@123 | Customer | Active |
| inactive@ambev.com | Test@123 | Customer | Inactive (login bloqueado) |
| suspended@ambev.com | Test@123 | Manager | Suspended (login bloqueado) |

### Cenarios de teste cobertos pelo seed

| # | Cenario | Sale Number |
|---|---------|-------------|
| 1 | Venda sem desconto (qty < 4) | SALE-001 |
| 2 | Desconto 10% (4-9 itens) | SALE-002 |
| 3 | Desconto 20% (10-20 itens) | SALE-003 |
| 4 | Mix de descontos em itens diferentes | SALE-004 |
| 5 | Venda cancelada | SALE-005 |
| 6 | Venda com item cancelado parcialmente | SALE-006 |
| 7 | Venda de filial diferente (Salvador) | SALE-007 |
| 8 | Venda de valor alto (R$ 5.600) | SALE-008 |
| 9 | Quantidade no limite (20 unidades) | SALE-009 |
| 10 | Venda criada hoje (filtro por data) | SALE-010 |

### Rodar Backend local (sem Docker)

```bash
cd template/backend/src/Ambev.DeveloperEvaluation.WebApi

# Requer PostgreSQL, MongoDB e Redis rodando local (ou via docker-compose apenas os bancos)
dotnet run
```

A API sobe em `http://localhost:8080` com Swagger habilitado.

### Rodar Frontend local (sem Docker)

```bash
cd template/frontend
npm install
npm start
```

O app sobe em `http://localhost:4200` e aponta para a API em `http://localhost:8080/api`.

---

## Stack Tecnologica

| Camada | Tecnologia |
|--------|-----------|
| Backend | .NET 8, C# 12, ASP.NET Core Web API |
| ORM | Entity Framework Core + Npgsql |
| Banco relacional | PostgreSQL 13 |
| NoSQL | MongoDB 8.0 |
| Cache | Redis 7.4 (StackExchange.Redis) |
| Mensageria interna | MediatR (in-process) |
| Validacao | FluentValidation |
| Mapeamento | AutoMapper |
| Autenticacao | JWT Bearer |
| Hash de senha | BCrypt.Net |
| Logs | Serilog |
| Frontend | Angular 21, TypeScript 5.9 |
| CSS | TailwindCSS 4 |
| Containerizacao | Docker, Docker Compose |
| Servidor web (frontend) | Nginx Alpine |
