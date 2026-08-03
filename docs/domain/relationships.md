# Relationships

**Fonte da verdade:** derivado diretamente das propriedades de cada entidade em
`src/BeeDay.Domain/Entities/` e `src/BeeDay.Domain/Experience/`, e dos 8 arquivos
`I*Repository.cs` em `src/BeeDay.Application/Common/Contracts/`.

## Mapa de Aggregate Roots

```mermaid
erDiagram
    User ||--o| UserExperience : "owned type (mesma PK)"
    User ||--o{ UserToken : "UserId"
    User ||--o{ Habit : "UserId (via Activity)"
    User ||--o{ RecurringTask : "UserId (via Activity)"
    User ||--o{ Project : "UserId (via Activity)"
    User ||--o| Wallet : "UserId (no máx. 1, por convenção de Application + índice único)"
    User ||--o{ WalletTag : "UserId"
    Project ||--o{ Todo : "ProjectId (composição real)"
    Wallet ||--o{ Transaction : "WalletId"
    WalletTag |o--o{ Transaction : "WalletTagId (opcional)"
    UserExperience ||--o{ ExperienceEntry : "entries (lista imutável)"
```

## Ownership (quem é "dono" de quem)

| Entidade | Dono | Como |
|---|---|---|
| `UserToken` | `User` | `UserId` |
| `Habit` | `User` | `UserId` (herdado de `Activity`, via `AssignOwner`) |
| `RecurringTask` | `User` | idem |
| `Project` | `User` | idem |
| `Todo` | `Project` (não diretamente `User`, apesar de também ter `UserId`) | `ProjectId`, atribuído exclusivamente por `Project.AddTodo` → `Todo.AssignTo` |
| `Wallet` | `User` | `UserId` |
| `WalletTag` | `User` | `UserId` |
| `Transaction` | `Wallet` (não diretamente `User` — `Transaction` não tem campo `UserId`) | `WalletId` |
| `UserExperience` | `User` | Owned Type, mesma PK |
| `ExperienceEntry` | `User` (via `UserExperience.Entries`) | Lista, sem FK própria exposta no Domain |

**Observação:** `Transaction` é o único Aggregate Root sem `UserId` direto — sua propriedade de
usuário é sempre alcançada indiretamente via `Wallet.UserId`. Isso é reforçado em
`ITransactionRepository`, cujos métodos recebem `userId` como parâmetro para autorização, mas o
Domain em si não guarda esse dado na entidade `Transaction`.

## Composição real vs. referência por Guid

Este Domain tem exatamente **uma** relação de composição real (coleção de objetos filhos vivendo
dentro do agregado pai em memória): `Project.Todos` (`List<Todo>`). Toda outra "relação" entre
Aggregate Roots é uma referência por `Guid`, resolvida por consulta separada via repositório —
nunca uma referência de objeto a objeto entre dois Aggregate Roots.

```mermaid
graph TD
    subgraph "Composição real (objeto vive dentro do agregado pai)"
        Project -->|List Todos| Todo
        User -->|Owned Type| UserExperience
        UserExperience -->|lista imutável| ExperienceEntry
    end
    subgraph "Referência por Guid (agregados independentes)"
        Habit -.UserId.-> User
        RecurringTask -.UserId.-> User
        Project -.UserId.-> User
        Wallet -.UserId.-> User
        WalletTag -.UserId.-> User
        UserToken -.UserId.-> User
        Transaction -.WalletId.-> Wallet
        Transaction -.WalletTagId opcional.-> WalletTag
    end
```

## Hierarquia de herança (`Activity`)

```mermaid
classDiagram
    class Entity {
        <<abstract>>
        +Guid Id
    }
    class Activity {
        <<abstract>>
        +Guid UserId
        +string Title
        +bool Completed
    }
    class Habit
    class RecurringTask
    class Project
    class Todo
    class Wallet
    class WalletTag
    class Transaction
    class UserToken
    class User
    class ExperienceEntry

    Entity <|-- Activity
    Activity <|-- Habit
    Activity <|-- RecurringTask
    Activity <|-- Project
    Activity <|-- Todo
    Entity <|-- Wallet
    Entity <|-- WalletTag
    Entity <|-- Transaction
    Entity <|-- UserToken
    Entity <|-- User
    Entity <|-- ExperienceEntry
```

Apenas 4 das 10 classes que herdam de `Entity` passam por `Activity`; as outras 6 (`Wallet`,
`WalletTag`, `Transaction`, `UserToken`, `User`, `ExperienceEntry`) herdam diretamente de `Entity`.

## Dependências do Domain

Confirmado (Sprint 16.3, reconfirmado nesta Sprint): `src/BeeDay.Domain` não tem nenhuma
`ProjectReference` e nenhum `using` para `Microsoft.EntityFrameworkCore`/`Microsoft.AspNetCore`.
As únicas dependências internas são entre os próprios namespaces do Domain:

```mermaid
graph LR
    Abstractions --> Exceptions
    Entities --> Abstractions
    Entities --> Common
    Entities --> Enums
    Entities --> ValueObjects
    Entities --> Experience
    Experience --> Abstractions
    Experience --> Enums
    Experience --> Exceptions
    Experience --> Common
    ValueObjects --> Exceptions
    Common --> Exceptions
    Events --> Enums
```

`Events` é o único namespace que não depende de `Entities`/`Experience` — os registros de evento
são independentes de qualquer entidade concreta (recebem os dados já extraídos via construtor).

## Fronteiras de transação (verificado em `EfUnitOfWork`, Infrastructure)

Não é uma regra de Domain, mas relevante para entender até onde uma mudança em um agregado pode
ser atômica com outro: `docs/architecture/06-persistence-architecture.md` §8 documenta que
`IUnitOfWork` coordena os 8 repositórios contra um único `DbContext`. O próprio Domain não impõe
nenhuma regra de atomicidade entre agregados — isso é inteiramente uma decisão de Application/
Infrastructure.

## Fontes de verdade

**Arquivos consultados:** todas as entidades em `src/BeeDay.Domain/Entities/` e
`src/BeeDay.Domain/Experience/`, os 8 arquivos `I*Repository.cs` em
`src/BeeDay.Application/Common/Contracts/`.
**Entidades relacionadas:** todos os documentos individuais de agregado em `docs/domain/`.
**Documentação relacionada:** `docs/architecture/04-dependency-rules.md` (dependências entre
camadas, diferente das dependências internas ao Domain documentadas aqui),
`docs/architecture/06-persistence-architecture.md` §8 (`IUnitOfWork`).
