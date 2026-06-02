# Plano de Implementacao - Sales API (Ambev Developer Evaluation)

## Contexto

Este e um projeto de avaliacao tecnica para desenvolvedores senior da Ambev (DeveloperStore). O template fornece uma base em .NET 8.0/C# com DDD ja implementado para Users/Auth. O candidato precisa implementar uma **API de Vendas (Sales)** com CRUD completo, regras de negocio de desconto por quantidade, e publicacao de eventos de dominio. O objetivo e demonstrar proficiencia em DDD, CQRS, testes, e boas praticas de engenharia de software.

**Diretorio base**: `/Users/sirlanmoraes/Documents/areatrab/mouts/mouts/template/backend/`

---

## Skills Avaliadas (22) - Mapeamento de Cobertura

| #   | Skill                                | Como cobrimos                                                |
| --- | ------------------------------------ | ------------------------------------------------------------ |
| 1   | C# e .NET 8.0                        | Toda a implementacao                                         |
| 2   | Separacao de camadas                 | DDD: Domain, Application, ORM, WebApi, IoC                   |
| 3   | PostgreSQL **e MongoDB**             | PostgreSQL (write) + MongoDB (event store + read model)      |
| 4   | Design patterns (Mediator)           | CQRS via MediatR                                             |
| 5   | ORM (EF Core)                        | Mapeamento Sales/SaleItems                                   |
| 6   | Unit tests (xUnit)                   | Cobertura completa com boundary values                       |
| 7   | Mocking (NSubstitute)                | Handler tests                                                |
| 8   | AutoMapper                           | Profiles em todas as camadas                                 |
| 9   | API RESTful                          | SalesController com CRUD + Swagger documentado + Angular consumindo |
| 10  | Git                                  | Git Flow + semantic commits                                  |
| 11  | Relational + Non-relational          | PostgreSQL + MongoDB                                         |
| 12  | Faker (Bogus)                        | Test data generators                                         |
| 13  | Organizacao de projeto               | Mesma estrutura do template                                  |
| 14  | Paginacao, filtragem, ordenacao      | GetSales com \_page, \_size, \_order, filtros                |
| 15  | Error handling + response formatting | ApiResponse/ApiResponseWithData envelopes                    |
| 16  | **Git Flow + Semantic Commits**      | Branches: develop, feat/, test/ + commits feat:, fix:, test: |
| 17  | **Performance optimization**         | Redis cache para consultas frequentes + indices DB           |
| 18  | Async programming                    | async/await + CancellationToken em tudo                      |
| 19  | Code quality                         | FluentValidation, clean code, DDD correto                    |
| 20  | Problem-solving                      | Regras de desconto no dominio, Expression trees              |
| 21  | Atencao a detalhe                    | Boundary values, External Identities, soft delete            |
| 22  | **Integrar multiplas tecnologias**   | PostgreSQL + MongoDB + Redis + Rebus + Angular + Swagger     |

---

## Estrategia para se Destacar

1. **Rich Domain Model** - Regras de negocio (descontos, limites) dentro das entidades de dominio, nao nos handlers
2. **Aggregate Root** - Sale controla seus invariantes; Items acessiveis apenas via metodos do aggregate
3. **External Identities** - Customer e Branch como referencias desnormalizadas (DDD correto)
4. **CQRS Completo** - PostgreSQL para escrita, MongoDB para read model de consultas
5. **Event Store** - Domain events persistidos no MongoDB (rastreabilidade)
6. **Redis Cache** - Cache inteligente para GetSale e GetSales com invalidacao
7. **Rebus** - Service bus para publicacao de domain events (mesmo que in-memory)
8. **Dynamic Queries** - Paginacao, ordenacao e filtragem com Expression trees
9. **Cobertura de testes** - Boundary value analysis nos tiers de desconto (3, 4, 9, 10, 19, 20, 21)
10. **Git Flow completo** - develop, feature branches, semantic commits
11. **Swagger documentado** - XML comments, exemplos, JWT auth no Swagger UI
12. **Frontend Angular completo** - Login com JWT, CRUD de vendas, preview de descontos, paginacao

---

## Fase 0 - Git Flow Setup

### Estrategia de branches:

```
main
└── develop
    ├── feat/sales-domain          (Fase 1 - Domain Layer)
    ├── feat/sales-orm             (Fase 2 - ORM + PostgreSQL)
    ├── feat/sales-mongodb         (Fase 3 - MongoDB Event Store + Read Model)
    ├── feat/sales-application     (Fase 4 - Application/CQRS)
    ├── feat/sales-redis-cache     (Fase 5 - Redis Cache)
    ├── feat/sales-rebus-events    (Fase 6 - Rebus Event Publishing)
    ├── feat/sales-api             (Fase 7 - WebApi)
    └── feat/sales-tests           (Fase 8 - Testes)
```

### Padrao de commits (Conventional Commits):

- `feat: add Sale and SaleItem domain entities with discount rules`
- `feat: add SaleValidator and SaleItemValidator`
- `feat: add MongoDB event store for domain events`
- `feat: add Redis cache for sale queries`
- `feat: add Rebus event publishing for domain events`
- `test: add SaleItem discount boundary value tests`
- `chore: register Sale dependencies in IoC`
- `docs: update README with Sales API documentation`

---

## Fase 1 - Domain Layer (~15 arquivos novos)

### 1.1 Enum

- `src/.../Domain/Enums/SaleStatus.cs` - `Unknown = 0, Active, Cancelled`

### 1.2 Entidades

**`src/.../Domain/Entities/SaleItem.cs`** (entidade filha):

- Propriedades: `SaleId`, `ProductExternalId`, `ProductName`, `Quantity`, `UnitPrice`, `Discount`, `TotalAmount`, `IsCancelled`, `CreatedAt`, `UpdatedAt`
- Metodo `CalculateDiscount()` com regras de negocio:
  - Qty > 20 → `DomainException`
  - Qty >= 10 → 20%
  - Qty >= 4 → 10%
  - Qty < 4 → 0% (desconto nao permitido)
- Metodo `CalculateTotalAmount()` → `Quantity * UnitPrice * (1 - Discount)`
- Metodo `Cancel()` → seta `IsCancelled = true`, `UpdatedAt = UtcNow`

**`src/.../Domain/Entities/Sale.cs`** (aggregate root):

- Propriedades: `SaleNumber`, `SaleDate`, `CustomerExternalId`, `CustomerName`, `BranchExternalId`, `BranchName`, `TotalAmount`, `Status`, `Items` (IReadOnlyCollection exposto, List interno privado), `CreatedAt`, `UpdatedAt`
- Constructor: `CreatedAt = DateTime.UtcNow`, `Status = Active`, `Items = new List`
- Metodos: `AddItem()` (calcula desconto + total), `CancelItem(itemId)`, `Cancel()` (cancela tudo), `RecalculateTotal()`

### 1.3 Validators

- `src/.../Domain/Validation/SaleValidator.cs` - SaleNumber NotEmpty, Customer/Branch NotEmpty, Status != Unknown, Items NotEmpty
- `src/.../Domain/Validation/SaleItemValidator.cs` - Qty > 0 e <= 20, UnitPrice > 0, ProductName NotEmpty

### 1.4 Domain Events (4 arquivos)

- `SaleCreatedEvent.cs`, `SaleModifiedEvent.cs`, `SaleCancelledEvent.cs`, `ItemCancelledEvent.cs`
- Cada um segue o padrao `UserRegisteredEvent`: classe simples com referencia ao aggregate
- Adicionalmente implementam `INotification` (MediatR) para serem publicados via Rebus

### 1.5 Repository Interfaces

- `src/.../Domain/Repositories/ISaleRepository.cs` - CRUD + `GetQueryable()` (PostgreSQL)
- `src/.../Domain/Repositories/ISaleEventStore.cs` - `StoreEventAsync(object domainEvent)`, `GetEventsAsync(Guid saleId)` (MongoDB)
- `src/.../Domain/Repositories/ISaleReadRepository.cs` - `GetByIdAsync`, `GetPaginatedAsync` (MongoDB read model)

### 1.6 Specification

- `src/.../Domain/Specifications/ActiveSaleSpecification.cs` - `IsSatisfiedBy` → Status == Active

---

## Fase 2 - ORM Layer / PostgreSQL (3 novos + 2 modificados)

### 2.1 Entity Configurations

- `src/.../ORM/Mapping/SaleConfiguration.cs` - Table "Sales", UUID PK com `gen_random_uuid()`, Status como string, HasMany Items com Cascade, Index unico em SaleNumber, indices em CustomerExternalId e BranchExternalId para performance
- `src/.../ORM/Mapping/SaleItemConfiguration.cs` - Table "SaleItems", UUID PK, Precision(18,2) para valores monetarios, Index em SaleId

### 2.2 Modificacoes

- `DefaultContext.cs` - Adicionar `DbSet<Sale>` e `DbSet<SaleItem>`

### 2.3 Repository

- `src/.../ORM/Repositories/SaleRepository.cs` - Implementa ISaleRepository com `Include(s => s.Items)` nos Gets, `AsNoTracking()` nos queries de leitura

### 2.4 Migration

- `dotnet ef migrations add AddSalesDomain`

---

## Fase 3 - MongoDB Layer (Event Store + Read Model) (~5 arquivos novos)

### 3.1 Pacotes NuGet necessarios

- `MongoDB.Driver` no projeto ORM

### 3.2 MongoDB Context

- `src/.../ORM/MongoDB/MongoDbContext.cs` - Configuracao do client MongoDB, connection string do appsettings

### 3.3 Event Store (persistencia de domain events)

- `src/.../ORM/MongoDB/Documents/SaleEventDocument.cs` - Documento MongoDB:
  ```
  { Id, SaleId, EventType, EventData (JSON), OccurredAt, SaleNumber }
  ```
- `src/.../ORM/MongoDB/Repositories/SaleEventStoreRepository.cs` - Implementa `ISaleEventStore`
  - `StoreEventAsync`: serializa o evento e salva no MongoDB
  - `GetEventsAsync(saleId)`: retorna historico de eventos de uma venda

### 3.4 Read Model (desnormalizacao para consultas rapidas)

- `src/.../ORM/MongoDB/Documents/SaleReadModel.cs` - Documento MongoDB desnormalizado com todos os dados da venda + items inline (sem joins)
- `src/.../ORM/MongoDB/Repositories/SaleReadRepository.cs` - Implementa `ISaleReadRepository`
  - `UpsertAsync(Sale)`: atualiza o read model quando a venda muda
  - `GetByIdAsync`: busca rapida por Id
  - `GetPaginatedAsync`: listagem com filtros, ordenacao e paginacao direto no MongoDB

### 3.5 Configuracao

- Adicionar connection string do MongoDB no `appsettings.json`:
  ```json
  "MongoDB": {
    "ConnectionString": "mongodb://developer:ev@luAt10n@localhost:27017",
    "DatabaseName": "developer_evaluation"
  }
  ```

---

## Fase 4 - Application Layer (CQRS) (~32 arquivos novos)

### 4.1 CreateSale

- Command (class) com List<CreateSaleItemDto>, Validator, Handler
- Handler flow: validate → map → AddItem para cada item (aplica desconto) → save no PostgreSQL → publicar SaleCreatedEvent via Rebus → atualizar read model MongoDB → return Result (Guid Id)

### 4.2 GetSale

- Command (record com Guid Id), Handler busca do **MongoDB read model** (rapido, sem joins)
- Fallback para PostgreSQL se nao encontrar no read model (resiliencia)
- Resultado cacheado no **Redis**

### 4.3 GetSales (Listagem)

- Command com: Page, Size, Order (string), filtros opcionais
- Handler usa **MongoDB read model** para consultas paginadas
- `QueryableExtensions` para ordenacao dinamica e filtragem
- Resultado cacheado no **Redis** com key baseada nos parametros de query

### 4.4 UpdateSale

- Command com Id + campos mutaveis + List<UpdateSaleItemDto>
- Handler: busca do PostgreSQL, valida Active (Specification), aplica mudancas, recalcula, persiste, publica SaleModifiedEvent via Rebus, atualiza read model, invalida cache Redis

### 4.5 DeleteSale (Cancel)

- Soft delete: chama sale.Cancel(), persiste, publica SaleCancelledEvent via Rebus, atualiza read model, invalida cache Redis

### 4.6 CancelSaleItem

- Command com SaleId + ItemId
- Handler: busca sale, chama sale.CancelItem(itemId), persiste, publica ItemCancelledEvent via Rebus, atualiza read model, invalida cache Redis

### 4.7 Utilitarios

- `src/.../Application/Common/QueryableExtensions.cs` - Ordenacao dinamica e filtragem via Expression trees
- `src/.../Application/Common/CacheKeys.cs` - Constantes para chaves do Redis

---

## Fase 5 - Redis Cache (~3 arquivos novos)

### 5.1 Pacotes NuGet

- `StackExchange.Redis` ou `Microsoft.Extensions.Caching.StackExchangeRedis` no projeto Common ou ORM

### 5.2 Implementacao

- `src/.../Common/Caching/ICacheService.cs` - Interface: `GetAsync<T>`, `SetAsync<T>`, `RemoveAsync`, `RemoveByPrefixAsync`
- `src/.../ORM/Caching/RedisCacheService.cs` - Implementacao com `IDistributedCache` ou `IConnectionMultiplexer`

### 5.3 Estrategia de cache

- **GetSale(id)**: cache por 5min, key = `sale:{id}`
- **GetSales(query)**: cache por 2min, key = `sales:{hash_dos_parametros}`
- **Invalidacao**: nos handlers de Create/Update/Cancel, remover `sale:{id}` e `sales:*` (prefix-based)

### 5.4 Configuracao

- `appsettings.json`:
  ```json
  "Redis": {
    "ConnectionString": "localhost:6379,password=ev@luAt10n"
  }
  ```

---

## Fase 6 - Rebus Event Publishing (~4 arquivos novos)

### 6.1 Pacotes NuGet

- `Rebus` + `Rebus.ServiceProvider` no projeto Application/Common
- Pode usar `Rebus.Transport.InMem` (in-memory transport) ja que nao e obrigatorio um broker real

### 6.2 Event Handlers (subscribers)

- `src/.../Application/Sales/Events/SaleCreatedEventHandler.cs` - Recebe SaleCreatedEvent, loga via ILogger, salva no MongoDB event store, atualiza read model
- `src/.../Application/Sales/Events/SaleModifiedEventHandler.cs` - Idem para modificacao
- `src/.../Application/Sales/Events/SaleCancelledEventHandler.cs` - Idem para cancelamento
- `src/.../Application/Sales/Events/ItemCancelledEventHandler.cs` - Idem para cancelamento de item

### 6.3 Configuracao do Rebus

- No `InfrastructureModuleInitializer` ou `ApplicationModuleInitializer`:
  ```csharp
  builder.Services.AddRebus(configure => configure
      .Transport(t => t.UseInMemoryTransport(new InMemNetwork(), "sales-queue"))
      .Routing(r => r.TypeBased().MapAssemblyOf<SaleCreatedEvent>("sales-queue")));
  ```

### 6.4 Publicacao nos Handlers

- Apos persistir no PostgreSQL, o handler chama `await _bus.Publish(new SaleCreatedEvent(sale))`
- O event handler do Rebus recebe e: (1) loga, (2) salva no event store MongoDB, (3) atualiza read model MongoDB

---

## Fase 7 - WebApi Layer (~27 arquivos novos)

### 7.1 Controller

- `src/.../WebApi/Features/Sales/SalesController.cs`
- Endpoints:
  - `POST /api/sales` → CreateSale
  - `GET /api/sales/{id}` → GetSale
  - `GET /api/sales` → GetSales (list com query params: `_page`, `_size`, `_order`, filtros)
  - `PUT /api/sales/{id}` → UpdateSale
  - `DELETE /api/sales/{id}` → CancelSale (soft delete)
  - `PATCH /api/sales/{saleId}/items/{itemId}/cancel` → CancelItem

### 7.2 Feature DTOs (pasta por operacao)

- CreateSale: Request (com List<CreateSaleItemRequest>), RequestValidator, Response, Profile
- GetSale: Request, RequestValidator, Response (com List<GetSaleItemResponse>), Profile
- GetSales: Request (com parametros de query), RequestValidator, Response, Profile
- UpdateSale: Request (com List<UpdateSaleItemRequest>), RequestValidator, Response, Profile
- DeleteSale: Request, RequestValidator, Profile
- CancelSaleItem: Request, RequestValidator, Response, Profile

---

## Fase 8 - Testes (~15+ arquivos novos)

### 8.1 Domain Entity Tests

- `SaleTests.cs`:
  - `Given_NewSale_When_Created_Then_StatusShouldBeActive`
  - `Given_ActiveSale_When_Cancelled_Then_StatusShouldBeCancelled`
  - `Given_ActiveSale_When_Cancelled_Then_AllItemsShouldBeCancelled`
  - `Given_SaleWithItems_When_ItemCancelled_Then_TotalShouldBeRecalculated`
  - `Given_ValidSaleData_When_Validated_Then_ShouldReturnValid`
  - `Given_Sale_When_AddingItem_Then_TotalShouldBeUpdated`

- `SaleItemTests.cs` (boundary value analysis completa):
  - `Given_ItemWithQuantity1_When_DiscountCalculated_Then_DiscountShouldBeZero`
  - `Given_ItemWithQuantity3_When_DiscountCalculated_Then_DiscountShouldBeZero`
  - `Given_ItemWithQuantity4_When_DiscountCalculated_Then_DiscountShouldBe10Percent`
  - `Given_ItemWithQuantity9_When_DiscountCalculated_Then_DiscountShouldBe10Percent`
  - `Given_ItemWithQuantity10_When_DiscountCalculated_Then_DiscountShouldBe20Percent`
  - `Given_ItemWithQuantity19_When_DiscountCalculated_Then_DiscountShouldBe20Percent`
  - `Given_ItemWithQuantity20_When_DiscountCalculated_Then_DiscountShouldBe20Percent`
  - `Given_ItemWithQuantity21_When_DiscountCalculated_Then_ShouldThrowDomainException`
  - `Given_ActiveItem_When_Cancelled_Then_IsCancelledShouldBeTrue`
  - `Given_Item_When_TotalCalculated_Then_ShouldApplyDiscountCorrectly`

### 8.2 Validator Tests

- `SaleValidatorTests.cs` - FluentValidation.TestHelper
- `SaleItemValidatorTests.cs` - FluentValidation.TestHelper

### 8.3 Specification Tests

- `ActiveSaleSpecificationTests.cs` - Theory com InlineData por SaleStatus

### 8.4 Handler Tests (NSubstitute + FluentAssertions)

- `CreateSaleHandlerTests.cs` - Sucesso, validacao, aplicacao de descontos, evento publicado
- `CancelSaleHandlerTests.cs` - Sucesso, nao encontrado, ja cancelado
- `CancelSaleItemHandlerTests.cs` - Sucesso, item ja cancelado, recalculo total

### 8.5 Test Data

- `SaleTestData.cs` - Bogus Faker<Sale>
- `SaleItemTestData.cs` - Bogus Faker<SaleItem> com `GenerateValidItem(int quantity)`

---

## Fase 9 - Swagger Completo

### 9.1 Melhorias no Program.cs
- Configurar `AddSwaggerGen` com:
  ```csharp
  builder.Services.AddSwaggerGen(c =>
  {
      c.SwaggerDoc("v1", new OpenApiInfo
      {
          Title = "Ambev Developer Evaluation API",
          Version = "v1",
          Description = "API de vendas com regras de desconto por quantidade"
      });
      c.IncludeXmlComments(xmlFilePath); // habilitar XML docs
      c.AddSecurityDefinition("Bearer", ...); // JWT no Swagger
      c.AddSecurityRequirement(...);
  });
  ```

### 9.2 Habilitar XML Comments nos .csproj
- WebApi.csproj: `<GenerateDocumentationFile>true</GenerateDocumentationFile>`

### 9.3 Documentar endpoints do SalesController
- `/// <summary>` em cada action com descricao clara
- `/// <remarks>` com exemplos de request JSON
- `/// <response code="200">` para cada status code
- `[ProducesResponseType]` detalhados com tipos

### 9.4 Documentar endpoints do UsersController e AuthController
- Melhorar a documentacao existente com exemplos

---

## Fase 10 - Frontend Angular

### 10.1 Setup do projeto
- `template/frontend/` - novo projeto Angular (ng new)
- Angular 17+ com standalone components
- Estrutura:
  ```
  frontend/
  ├── src/
  │   ├── app/
  │   │   ├── core/                  (guards, interceptors, services base)
  │   │   │   ├── auth/
  │   │   │   │   ├── auth.service.ts        (login, logout, token management)
  │   │   │   │   ├── auth.guard.ts          (protege rotas autenticadas)
  │   │   │   │   └── auth.interceptor.ts    (adiciona JWT header em requests)
  │   │   │   └── services/
  │   │   │       └── api.service.ts         (HttpClient base com error handling)
  │   │   ├── features/
  │   │   │   ├── login/
  │   │   │   │   ├── login.component.ts     (formulario email + password)
  │   │   │   │   └── login.component.html
  │   │   │   └── sales/
  │   │   │       ├── sales.service.ts       (CRUD calls para /api/sales)
  │   │   │       ├── sales-list/
  │   │   │       │   ├── sales-list.component.ts   (tabela paginada com filtros)
  │   │   │       │   └── sales-list.component.html
  │   │   │       ├── sales-form/
  │   │   │       │   ├── sales-form.component.ts   (criar/editar venda + items)
  │   │   │       │   └── sales-form.component.html
  │   │   │       └── sales-detail/
  │   │   │           ├── sales-detail.component.ts (detalhe + cancelar items)
  │   │   │           └── sales-detail.component.html
  │   │   ├── shared/                (componentes reutilizaveis)
  │   │   │   ├── pagination/
  │   │   │   └── confirm-dialog/
  │   │   ├── app.component.ts
  │   │   ├── app.routes.ts
  │   │   └── app.config.ts
  │   └── environments/
  │       ├── environment.ts          (apiUrl: http://localhost:8080)
  │       └── environment.prod.ts
  ├── angular.json
  ├── package.json
  └── Dockerfile
  ```

### 10.2 Core - Autenticacao
- **auth.service.ts**: `login(email, password)` → `POST /api/auth` → armazena JWT no localStorage → BehaviorSubject para estado de autenticacao
- **auth.guard.ts**: CanActivate que verifica se tem token valido, redireciona para /login se nao
- **auth.interceptor.ts**: HttpInterceptor que adiciona `Authorization: Bearer {token}` em todas as requests

### 10.3 Tela de Login
- Formulario com email + password (Reactive Forms com validacao)
- Chama `POST /api/auth` com credenciais
- Sucesso → redireciona para `/sales`
- Erro → mostra mensagem de erro

### 10.4 Tela de Listagem de Vendas (sales-list)
- Tabela com colunas: SaleNumber, Date, Customer, Branch, Total, Status, Acoes
- Paginacao usando `_page` e `_size`
- Ordenacao clicando nas colunas (envia `_order`)
- Filtros por: customer name, branch, status, data range
- Botoes: "Nova Venda", "Ver", "Editar", "Cancelar" por linha
- Badge de status (Active = verde, Cancelled = vermelho)

### 10.5 Tela de Criar/Editar Venda (sales-form)
- Formulario com:
  - SaleNumber (auto-gerado ou editavel)
  - Data da venda (datepicker)
  - Customer (ExternalId + Name)
  - Branch (ExternalId + Name)
  - Tabela de items dinamica:
    - ProductName, ProductExternalId, Quantity, UnitPrice
    - Desconto e Total calculados automaticamente no frontend (preview)
    - Botao "Adicionar Item" e "Remover Item"
  - Total geral calculado
- Validacoes no frontend espelhando as do backend
- Preview dos descontos conforme digita quantidade (4+ → 10%, 10-20 → 20%, >20 → erro)

### 10.6 Tela de Detalhe da Venda (sales-detail)
- Dados completos da venda
- Lista de items com desconto e total por item
- Botao "Cancelar Item" individual (com confirmacao)
- Botao "Cancelar Venda" (com confirmacao)
- Historico de eventos (se exposto via API do MongoDB event store)

### 10.7 CORS no Backend
- Adicionar no `Program.cs`:
  ```csharp
  builder.Services.AddCors(options =>
  {
      options.AddPolicy("AllowAngular", policy =>
          policy.WithOrigins("http://localhost:4200")
                .AllowAnyHeader()
                .AllowAnyMethod());
  });
  // ...
  app.UseCors("AllowAngular");
  ```

### 10.8 Docker
- `frontend/Dockerfile` com multi-stage build (node → nginx)
- Adicionar servico no `docker-compose.yml`:
  ```yaml
  ambev.developerevaluation.frontend:
    container_name: ambev_developer_evaluation_frontend
    build:
      context: ./frontend
      dockerfile: Dockerfile
    ports:
      - "4200:80"
    depends_on:
      - ambev.developerevaluation.webapi
  ```

---

## Fase 11 - Docker e Configuracao Final

### 9.1 docker-compose.yml

- Ja tem PostgreSQL, MongoDB, Redis configurados
- Verificar que a WebApi tem `depends_on` para os servicos
- Adicionar network compartilhada se necessario
- Adicionar volume para persistencia do MongoDB

### 9.2 appsettings.json atualizado

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=ambev.developerevaluation.database;Database=developer_evaluation;Username=developer;Password=ev@luAt10n"
  },
  "MongoDB": {
    "ConnectionString": "mongodb://developer:ev%40luAt10n@ambev.developerevaluation.nosql:27017",
    "DatabaseName": "developer_evaluation"
  },
  "Redis": {
    "ConnectionString": "ambev.developerevaluation.cache:6379,password=ev@luAt10n"
  },
  "Jwt": { ... }
}
```

---

## IoC - Resumo de Registros

No `InfrastructureModuleInitializer.cs`:

```csharp
// PostgreSQL
services.AddScoped<ISaleRepository, SaleRepository>();

// MongoDB
services.AddSingleton<MongoDbContext>();
services.AddScoped<ISaleEventStore, SaleEventStoreRepository>();
services.AddScoped<ISaleReadRepository, SaleReadRepository>();

// Redis
services.AddSingleton<ICacheService, RedisCacheService>();

// Rebus
services.AddRebus(...);
```

---

## Ordem Final de Implementacao

**Regra de commits**: Mensagens pequenas, naturais, como um humano. Nenhuma mencao a IA ou mensagens gigantes.

1. **Git**: Criar branch `develop` a partir de `main`
2. **feat/sales-domain**: Fase 1 (Domain Layer) → merge em develop
3. **feat/sales-orm**: Fase 2 (ORM/PostgreSQL) → merge em develop
4. **feat/sales-mongodb**: Fase 3 (MongoDB) → merge em develop
5. **feat/sales-application**: Fase 4 (Application/CQRS) → merge em develop
6. **feat/sales-redis-cache**: Fase 5 (Redis) → merge em develop
7. **feat/sales-rebus-events**: Fase 6 (Rebus) → merge em develop
8. **feat/sales-api**: Fase 7 (WebApi) → merge em develop
9. **feat/sales-tests**: Fase 8 (Testes) → merge em develop
10. **feat/swagger-docs**: Fase 9 (Swagger Completo) → merge em develop
11. **feat/angular-frontend**: Fase 10 (Frontend Angular) → merge em develop
12. **chore/docker-config**: Fase 11 (Docker + Config Final) → merge em develop
13. **develop → main**: PR final

---

## Verificacao

1. **Build backend**: `dotnet build` sem erros
2. **Testes**: `dotnet test` passa todos (existentes + novos)
3. **Build frontend**: `ng build` sem erros
4. **Docker**: `docker-compose up` sobe todos os servicos (API + Angular + PostgreSQL + MongoDB + Redis)
5. **Login via Angular**: Acessar http://localhost:4200, fazer login com credenciais, verificar redirecionamento para /sales
6. **CRUD via Angular**:
   - Criar venda com itens de diferentes quantidades → verificar descontos no preview
   - Listar vendas com paginacao e filtros
   - Editar venda existente
   - Cancelar item individual → verificar recalculo do total
   - Cancelar venda → verificar status
7. **API via Swagger** (http://localhost:8080/swagger):
   - Testar autenticacao com JWT
   - Verificar documentacao completa com exemplos
   - Testar endpoints diretamente
8. **Boundary tests**: qty=21 → erro, qty=4 → 10%, qty=10 → 20%, qty=3 → sem desconto
9. **Git log**: verificar semantic commits e branch history limpo
