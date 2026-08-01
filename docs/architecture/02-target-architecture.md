# Arquitetura-Alvo

## 1. Visão geral

```text
┌───────────────────────────────────────────────────────────┐
│ LevelUp.Web                                               │
│ Blazor UI, HTTP endpoints, auth cookie, presenters        │
└───────────────────────┬───────────────────────────────────┘
                        │ usa contratos
┌───────────────────────▼───────────────────────────────────┐
│ LevelUp.Contracts                                         │
│ Requests, responses, IDs, enums externos, errors, paging │
└───────────────────────┬───────────────────────────────────┘
                        │ implementado por casos de uso
┌───────────────────────▼───────────────────────────────────┐
│ LevelUp.Application                                       │
│ Commands, queries, handlers, ports, policies             │
└───────────────────────┬───────────────────────────────────┘
                        │ coordena
┌───────────────────────▼───────────────────────────────────┐
│ LevelUp.Domain                                            │
│ Aggregates, entities, value objects, domain events       │
└───────────────────────▲───────────────────────────────────┘
                        │ persistido por adapters
┌───────────────────────┴───────────────────────────────────┐
│ LevelUp.Infrastructure                                    │
│ EF Core, JSON legado, e-mail, cache, clock, background   │
└───────────────────────────────────────────────────────────┘
```

## 2. Novo projeto `LevelUp.Contracts`

Responsabilidades:

- modelos de entrada e saída estáveis;
- contratos de paginação;
- envelopes de erro;
- identificadores e enums expostos ao consumidor;
- especificação de compatibilidade;
- tipos compartilháveis com clientes futuros;
- documentação XML pública.

Não deve conter:

- entidades de domínio;
- `DbContext`;
- atributos do EF Core;
- MediatR;
- componentes Blazor;
- serviços de infraestrutura;
- lógica de negócio.

## 3. Portas da Application

A Application define interfaces orientadas a capacidades, por exemplo:

```csharp
public interface IUserRepository
{
    Task<User?> GetByIdAsync(UserId id, CancellationToken ct);
    Task<User?> GetByEmailAsync(string normalizedEmail, CancellationToken ct);
    Task<bool> EmailExistsAsync(string normalizedEmail, CancellationToken ct);
    Task AddAsync(User user, CancellationToken ct);
}

public interface IActivityRepository
{
    Task<Habit?> GetHabitAsync(UserId userId, HabitId id, CancellationToken ct);
    Task<IReadOnlyList<Habit>> ListHabitsAsync(UserId userId, CancellationToken ct);
    Task AddHabitAsync(Habit habit, CancellationToken ct);
    Task RemoveHabitAsync(Habit habit, CancellationToken ct);
}

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
```

## 4. Queries de leitura

Para dashboards e listas, a Application deve usar read services projetados diretamente para contratos de saída:

```csharp
public interface IDashboardReadService
{
    Task<DashboardResponse> GetAsync(UserId userId, CancellationToken ct);
}
```

Isso evita carregar agregados completos apenas para exibição.

## 5. Escrita

Fluxo de escrita:

```text
UI request contract
  → command
  → validator
  → handler
  → repository port
  → domain aggregate
  → unit of work
  → response contract
```

## 6. Leitura

Fluxo de leitura:

```text
UI/query contract
  → query handler
  → read service
  → projection
  → response contract
```

## 7. Adapters de persistência

Durante a transição existirão:

```text
JsonUserRepository / JsonActivityRepository / ...
EfUserRepository   / EfActivityRepository   / ...
```

Ambos devem passar pela mesma suíte de testes de contrato.

## 8. Composição

A escolha do adapter deve ser por configuração:

```json
{
  "LevelUp": {
    "Persistence": {
      "Provider": "Json"
    }
  }
}
```

Depois:

```json
{
  "LevelUp": {
    "Persistence": {
      "Provider": "SqlServer"
    }
  }
}
```

Nenhum componente ou handler pode verificar o provider ativo.
