# Dependency Rules

**Fonte da verdade:** verificado diretamente nos 4 `.csproj` de `src/*` (seção `ProjectReference`),
por grep de namespaces cruzando camadas, e por leitura direta dos arquivos citados como exemplo.

## 1. Quem referencia quem (verificado via `ProjectReference` em cada `.csproj`)

| Projeto | Referencia (`ProjectReference`) |
|---|---|
| `BeeDay.Domain` | nenhum |
| `BeeDay.Application` | `BeeDay.Domain` |
| `BeeDay.Infrastructure` | `BeeDay.Application` (e transitivamente `BeeDay.Domain`) |
| `BeeDay.Web` | `BeeDay.Application`, `BeeDay.Domain`, `BeeDay.Infrastructure` |

## 2. Quem nunca referencia quem (verificado)

| Direção proibida | Como foi verificado |
|---|---|
| `BeeDay.Domain` → `Microsoft.EntityFrameworkCore` | `grep -r "Microsoft.EntityFrameworkCore" src/BeeDay.Domain` → 0 ocorrências |
| `BeeDay.Domain` → `Microsoft.AspNetCore` | `grep -r "Microsoft.AspNetCore" src/BeeDay.Domain` → 0 ocorrências |
| `BeeDay.Application` → `Microsoft.EntityFrameworkCore` | `grep -r "using Microsoft.EntityFrameworkCore" src/BeeDay.Application` → 0 ocorrências |
| `BeeDay.Application` → `BeeDay.Infrastructure` | Não existe `ProjectReference` de Application para Infrastructure em `BeeDay.Application.csproj` |
| `BeeDay.Web` → tipo concreto EF Core (`BeeDayDbContext` e afins) | `grep -r "BeeDayDbContext" src/BeeDay.Web` → 0 ocorrências; os tipos `Ef*Repository`/`BeeDayDbContext`/`EfUnitOfWork` são todos `internal sealed` em `BeeDay.Infrastructure`, inacessíveis fora do assembly exceto por `InternalsVisibleTo` (concedido só a projetos de teste, nunca a `BeeDay.Web` em código de produção) |

## 3. Exemplos reais

### 3.1 Domain não conhece infraestrutura

`src/BeeDay.Domain/Entities/Habit.cs:20`:

```csharp
public static Habit Create(string title, string? description, HabitDirection direction,
    HabitDifficulty difficulty, HabitResetCounter resetCounter, ActivityAttribute? attribute = null)
```

Um método estático de fábrica, sem `DbContext`, sem `HttpContext`, sem qualquer tipo de
Infrastructure ou Web — recebe e retorna apenas tipos do próprio Domain.

### 3.2 Application define a interface, Web implementa quando a tecnologia é "quem faz a requisição HTTP"

`src/BeeDay.Application/Common/Security/ICurrentUserContext.cs`:

```csharp
public interface ICurrentUserContext
{
    public Guid? UserId { get; }
}
```

`src/BeeDay.Web/Services/HttpCurrentUserContext.cs:6-16`:

```csharp
public sealed class HttpCurrentUserContext(IHttpContextAccessor httpContextAccessor) : ICurrentUserContext
{
    public Guid? UserId
    {
        get
        {
            var value = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(value, out var userId) ? userId : null;
        }
    }
}
```

Note que a implementação vive em `BeeDay.Web`, não em `BeeDay.Infrastructure` — porque
`HttpContext` é um conceito de apresentação/hospedagem web, não de infraestrutura de dados. Isso
mostra que "Infrastructure implementa o que Application define" não é absoluto: a implementação
vai para a camada que efetivamente possui a tecnologia concreta em questão.

### 3.3 Application define o repositório, Infrastructure implementa com EF Core

`src/BeeDay.Application/Common/Contracts/IHabitRepository.cs` declara a interface;
`src/BeeDay.Infrastructure/Persistence/SqlServer/Repositories/EfHabitRepository.cs` implementa com
`BeeDayDbContext`. `BeeDay.Web` só conhece `IHabitRepository` — nunca `EfHabitRepository`
diretamente (a classe é `internal`).

## 4. Regra de ownership (verificada, não apenas prescrita)

`CurrentUserGuard.RequireUserId` (`src/BeeDay.Application/Common/Security/CurrentUserGuard.cs`) é
o único ponto usado pelos Handlers para obter o `UserId` do usuário autenticado:

```csharp
public static Guid RequireUserId(ICurrentUserContext currentUser) =>
    currentUser.UserId ?? throw new InvalidDomainStateException("An authenticated User is required.");
```

Usado, por exemplo, em `CreateHabitCommandHandler`
(`src/BeeDay.Application/Features/Habits/Handlers/HabitCommandHandlers.cs:13-23`): o `UserId` do
novo hábito vem de `CurrentUserGuard.RequireUserId(currentUser)`, nunca de um campo do próprio
Request/Command — o cliente não pode escolher o proprietário do recurso.

## 5. Regra de modelos por fronteira

Cada fronteira tem seu próprio tipo, sem reaproveitamento:

| Fronteira | Tipo |
|---|---|
| Entrada de Application | `SaveHabitRequest`, `CreateHabitCommand` (`src/BeeDay.Application/Features/Habits/Requests/`, `Commands/`) |
| Domínio | `Habit` (`src/BeeDay.Domain/Entities/Habit.cs`) |
| Persistência EF Core | Mapeamento de `Habit` via `HabitConfiguration.cs`, mesma classe de Domain — não existe uma classe de "modelo de persistência" separada; o mapeamento Fluent API projeta o próprio agregado do Domain diretamente na tabela `Habits` |
| Saída de Application | `HabitResponse`/`DashboardResponse` (`src/BeeDay.Application/Features/*/Responses/`) |

Nota: diferente do que um documento de arquitetura antigo (agora em `docs/history/`) descrevia
como aspiração, não existe uma camada `Contract DTO` separada da camada de Command/Query — o
Request/Command da Application **é** o contrato de entrada.

## 6. Regra de transação

Verificado em `EfUnitOfWork` (`src/BeeDay.Infrastructure/Persistence/SqlServer/EfUnitOfWork.cs`):
um único `DbContext` é criado por instância do `IUnitOfWork` (via `IDbContextFactory<BeeDayDbContext>.CreateDbContext()`,
chamado uma vez no construtor) e compartilhado por todos os 8 repositórios que ele expõe;
`CommitTransactionAsync`/`RollbackTransactionAsync` operam sobre uma única transação ADO.NET
subjacente. `IUnitOfWork` é registrado `AddTransient` (não `AddScoped`), deliberadamente — segundo
comentário no código, para não viver pelo circuito inteiro do Blazor Server.

## 7. Regra de versionamento

Não verificada nesta Sprint contra código real — não há hoje nenhuma API pública versionada em
`src/BeeDay.Web` além do artefato OpenAPI estático em `docs/api/beeday.v1.yaml` (cujo conteúdo não
foi reauditado). Este item permanece um princípio declarado, não uma regra observável em código de
produção atual.
