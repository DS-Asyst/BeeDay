# Entities (que não são Aggregate Roots)

**Fonte da verdade:** verificado diretamente em `src/BeeDay.Domain/Entities/Activity.cs`,
`Entities/Todo.cs`, `Entities/Profile.cs`, `src/BeeDay.Domain/Experience/ExperienceEntry.cs`,
e os Handlers de Application citados por seção.

Este documento cobre entidades com identidade própria (`Guid Id`, exceto `Profile`, que não tem
identidade) que **não** são Aggregate Roots — ou porque são uma base abstrata compartilhada
(`Activity`), ou porque só existem dentro da fronteira de outro agregado (`Todo`, dentro de
`Project`; `ExperienceEntry`, dentro de `User`), ou porque são uma projeção computada
(`Profile`).

## Activity (base abstrata)

**Arquivo:** `src/BeeDay.Domain/Entities/Activity.cs`

- **Responsabilidade:** código compartilhado por `Habit`, `RecurringTask`, `Project`, `Todo` —
  título, descrição, `Featured`, `Attribute` (classificador opcional), `Completed`, timestamps.
- **Identidade:** herdada de `Entity` (`Guid Id`), mas `Activity` em si nunca é instanciada
  diretamente (classe `abstract`).
- **Ciclo de vida:** não tem ciclo de vida próprio — vive e morre com o subtipo concreto que a
  instancia.
- **Quem pode criá-la:** ninguém diretamente — só através de `Habit.Create`, `RecurringTask.Create`,
  `Project.Create`, ou `Todo.Create`.
- **Quem pode alterá-la:** os métodos protegidos/públicos que expõe são chamados pelos subtipos
  concretos (`UpdateDetails` é `protected`, chamado de dentro de cada `Update` do subtipo) ou
  diretamente pela Application via `AssignOwner`, `SetFeatured`, `SetAttribute`,
  `ToggleCompletion` (este último é `virtual` — `Project` o sobrescreve para sempre lançar).
- **Quem a possui:** o `UserId` é atribuído via `AssignOwner`, chamado por cada Handler de criação
  do subtipo concreto.

### Invariantes de `Activity`

1. **`UserId` obrigatório ao atribuir dono**: `AssignOwner` lança `ArgumentException` se
   `Guid.Empty`.
2. **Título e descrição sempre passam pelos Value Objects `ActivityTitle`/`ActivityDescription`**
   dentro de `UpdateDetails` — nunca atribuídos como string bruta.
3. **`Attribute`, se fornecido, deve ser um valor de enum válido** (`EnumValidation.Defined`).
4. **`ToggleCompletion()` simplesmente inverte o booleano** por padrão (`Completed = !Completed`) —
   `Project` é o único subtipo que sobrescreve esse comportamento (ver [`project.md`](project.md)
   invariante 2).
5. **`Touch()` é `protected`** — só o próprio subtipo (ou `Activity`) pode atualizar
   `UpdatedAtUtc`; a Application nunca define esse campo diretamente.

## Todo (entidade filha de Project)

**Arquivo:** `src/BeeDay.Domain/Entities/Todo.cs`

- **Responsabilidade:** um item de checklist dentro de um `Project`, com data de vencimento
  opcional.
- **Identidade:** `Guid Id` (herdado de `Entity`, via `Activity`).
- **Ciclo de vida:** criado via `Todo.Create(projectId, title, description, dueDate, attribute?)`,
  imediatamente anexado a um `Project` via `Project.AddTodo` (que reatribui `ProjectId` via
  `AssignTo`, redundante com o `projectId` já passado ao `Create` — mas é `AssignTo`, não o
  argumento do construtor, que é a fonte de verdade final, já que é `internal` e só chamável pelo
  próprio pacote Domain). Removido via `Project.RemoveTodo(todoId)`.
- **Quem pode criá-la:** apenas `CreateTodoCommandHandler`
  (`src/BeeDay.Application/Features/Todos/Handlers/TodoCommandHandlers.cs:22-24`) — chama
  `Todo.Create(...)`, depois `todo.AssignOwner(userId)`, e entrega o resultado a
  `IProjectRepository.AddTodoAsync`. Não existe nenhum outro ponto do código que instancie `Todo`.
- **Quem pode alterá-la:** `UpdateTodoCommandHandler` (`Update`, e movimentação entre Projects via
  `IProjectRepository.MoveTodoAsync`); `ToggleTodoCommandHandler` (`ToggleCompletion`, herdado de
  `Activity`, sem override).
- **Quem a possui:** `Project` (via `ProjectId`) — não tem repositório próprio; toda operação passa
  por `IProjectRepository`.

### Invariantes de `Todo`

1. **`ProjectId` nunca pode ser `Guid.Empty`** — verificado tanto em `Update` quanto em `AssignTo`
   (`internal`), ambos lançando `DomainValidationException` caso contrário.
2. **`Update` não move um To-Do entre Projects** — a troca de `ProjectId` só é permitida pelo
   caminho interno acionado por `Project.AddTodo`.
3. Invariantes herdadas de `Activity`, incluindo owner imutável após a primeira atribuição.

## Profile (view computada, não uma entidade persistida)

**Arquivo:** `src/BeeDay.Domain/Entities/Profile.cs`

- **Responsabilidade:** apresentar um subconjunto dos campos de `User` (nickname, nome, avatar,
  preferências, progresso) sem expor estado de autenticação. **Não é uma entidade EF-mapeada** —
  confirmado na Sprint 16.3: `UserConfiguration.cs` chama `Ignore()` para `HasProfile`/`Profile`.
- **Identidade:** nenhuma — `Profile` não herda de `Entity`, não tem `Id` próprio. É uma classe
  `sealed` comum com um construtor `internal`.
- **Ciclo de vida:** instanciada sob demanda a cada leitura de `User.Profile` (`new(...)` a cada
  chamada do getter) — não é persistida nem cacheada; não tem ciclo de vida independente do `User`
  que a gerou.
- **Quem pode criá-la:** apenas `User` (construtor `internal`, só chamável de dentro do assembly
  `BeeDay.Domain`; a única chamada real é o getter computado `User.Profile`).
- **Quem pode alterá-la:** ninguém — todas as propriedades são `get`-only, sem setters.
- **Quem a possui:** `User` (é uma projeção do próprio `User`, não uma entidade relacionada).

Na Sprint 30.5, o comentário XML obsoleto que justificava essa projeção pelo adapter JSON removido
foi substituído pela fronteira atual: código de perfil não deve adquirir nem mutar estado de
autenticação e segurança.

## ExperienceEntry (entidade dentro da fronteira de User)

Ver [`user.md`](user.md) §Experience para o detalhamento funcional completo. Resumo estrutural:

- **Arquivo:** `src/BeeDay.Domain/Experience/ExperienceEntry.cs`.
- **Identidade:** `Guid Id` (herdado de `Entity`).
- **Ciclo de vida:** construída por factory; `UserExperience.Add`/`TryAdd` é o fluxo funcional que
  calcula e fornece a transição. A factory revalida reward, total antes/depois, níveis e timestamp.
- **Quem pode criá-la:** qualquer consumer de Domain pode chamar a factory, mas não existe
  construtor público e estados inconsistentes são rejeitados.
- **Quem pode alterá-la:** ninguém — todas as propriedades são somente leitura após `Create`.
- **Quem a possui:** logicamente `User`, via `UserExperience.Entries`. No mapping relacional atual,
  a coleção não é hidratada; a divergência está registrada como `BD30-F030` para a Sprint 30.7.

## Fontes de verdade

**Arquivos consultados:** `src/BeeDay.Domain/Entities/Activity.cs`, `Entities/Todo.cs`,
`Entities/Profile.cs`, `Experience/ExperienceEntry.cs`,
`src/BeeDay.Application/Features/Todos/Handlers/TodoCommandHandlers.cs`,
`Features/Projects/Handlers/ProjectCommandHandlers.cs`,
`src/BeeDay.Infrastructure/Persistence/SqlServer/Configurations/UserConfiguration.cs` (citado
apenas para confirmar o `Ignore()` de `Profile`, fato já verificado na Sprint 16.3).
**Testes consultados:** `tests/BeeDay.Domain.Tests/ProjectTests.cs`,
`tests/BeeDay.Domain.Tests/ExperienceDomainTests.cs`.
**Entidades relacionadas:** [`project.md`](project.md), [`user.md`](user.md).
**Documentação relacionada:** [`business-rules.md`](business-rules.md),
`docs/architecture/06-persistence-architecture.md` §3 (Owned Type/Complex Type).
