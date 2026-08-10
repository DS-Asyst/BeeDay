# Domain

Documentação do `BeeDay.Domain` — reconstruída por completo na Sprint 16.4 a partir exclusivamente
do código atual (`src/BeeDay.Domain/`, `src/BeeDay.Application/`, `tests/BeeDay.Domain.Tests/`,
`tests/BeeDay.Application.Tests/`). Nenhuma afirmação vem de `docs/history/` ou de sprints
anteriores sem reverificação direta no código.

**Fonte da verdade:** cada documento abaixo declara individualmente as fontes exatas usadas para
validá-lo, na seção final "Fontes de verdade".

## Objetivo do domínio

O Domain do BeeDay modela hábitos, tarefas recorrentes, projetos com to-dos, uma carteira
financeira pessoal (Wallet), identidade de usuário e um sistema leve de progressão por experiência
(XP/Nível). Todo o Domain é puro C# — confirmado por ausência total de `using` para
`Microsoft.EntityFrameworkCore` ou `Microsoft.AspNetCore` em `src/BeeDay.Domain` (verificado na
Sprint 16.3, reconfirmado nesta Sprint).

## Organização

```text
src/BeeDay.Domain/
├── Abstractions/    Entity.cs — base de todo Aggregate Root e entidade filha
├── Entities/         8 Aggregate Roots + Activity (base abstrata) + Todo (filha) + Profile (view)
├── Enums/            12 enumerações usadas pelas entidades
├── Events/            3 Domain Events + IDomainEvent + DomainEvent (record base)
├── Exceptions/        DomainException (base), DomainValidationException, InvalidDomainStateException
├── Experience/        Subsistema de XP/Nível: UserExperience, ExperienceEntry, ExperienceReward,
│                       ExperienceSource, ExperienceCurve/LinearExperienceCurve/IExperienceCurve
├── ValueObjects/       6 Value Objects (ActivityTitle, ActivityDescription, EmailAddress,
│                       Nickname, ProjectColor, UserName)
└── Common/             EnumValidation.cs (utilitário interno)
```

## Aggregate Roots (8)

Identificados por possuírem uma interface de repositório dedicada em
`src/BeeDay.Application/Common/Contracts/` — o critério mais direto e verificável de "fronteira de
agregado" neste código, já que Domain não usa nenhum marcador explícito de `IAggregateRoot`.

| Aggregate | Documento |
|---|---|
| `User` | [`user.md`](user.md) |
| `UserToken` | [`user-token.md`](user-token.md) |
| `Habit` | [`habit.md`](habit.md) |
| `RecurringTask` | [`recurring-task.md`](recurring-task.md) |
| `Project` (com `Todo` como entidade filha) | [`project.md`](project.md) |
| `Wallet` | [`wallet.md`](wallet.md) |
| `WalletTag` | [`wallet-tag.md`](wallet-tag.md) |
| `Transaction` | [`transaction.md`](transaction.md) |

## Fronteiras de agregado

- `Habit`, `RecurringTask`, `Project`, `Todo` herdam da classe abstrata `Activity`
  (`Entities/Activity.cs`), que não é um Aggregate Root — é uma base de código compartilhado
  (título, descrição, `Featured`, `Attribute`, `Completed`, timestamps). Cada subtipo concreto é
  seu próprio Aggregate Root, exceto `Todo`.
- `Todo` **não** é um Aggregate Root — não tem repositório próprio; só é alcançável através de
  `IProjectRepository` (`AddTodoAsync`, `UpdateTodoAsync`, `MoveTodoAsync`, `RemoveTodoAsync`,
  `GetByTodoIdAsync`). Confirmado: o único ponto do código que chama `Todo.Create(...)` entrega o
  resultado imediatamente a `repository.AddTodoAsync(...)` — nunca existe um `Todo` fora da
  fronteira de `Project`.
- `UserExperience`/`ExperienceEntry` vivem dentro da fronteira de `User` — `UserExperience` é um
  Owned Type do EF Core mapeado com a mesma PK de `User` (confirmado na Sprint 16.3); não têm
  repositório próprio.
- `Wallet`, `WalletTag` e `Transaction` são 3 Aggregate Roots **separados**, não um agregado
  composto — cada um com seu próprio repositório e seu próprio ciclo de vida, ainda que
  logicamente relacionados (uma `Transaction` referencia `WalletId` e opcionalmente
  `WalletTagId`).

Ver [`relationships.md`](relationships.md) para o mapa completo de relacionamentos e ownership.

## Outros documentos

| Documento | Conteúdo |
|---|---|
| [`entities.md`](entities.md) | Entidades que não são Aggregate Roots: `Todo`, `Activity`, `Profile`, `ExperienceEntry` |
| [`value-objects.md`](value-objects.md) | Os 6 Value Objects + `ExperienceReward`/`ExperienceSource` |
| [`domain-events.md`](domain-events.md) | Os 3 Domain Events, quem publica, quem consome, diagrama de fluxo |
| [`business-rules.md`](business-rules.md) | Toda invariante extraída do código, com arquivo/método/teste |
| [`relationships.md`](relationships.md) | Diagramas Mermaid de agregados, ownership, referências, composição |

## Como navegar pela documentação

1. Comece por este `README.md` para a visão de fronteiras.
2. Leia [`relationships.md`](relationships.md) para o mapa visual antes de mergulhar em um
   agregado específico.
3. Leia o documento do Aggregate Root que interessa (tabela acima).
4. Consulte [`entities.md`](entities.md) e [`value-objects.md`](value-objects.md) para os tipos
   auxiliares referenciados pelos agregados.
5. Consulte [`domain-events.md`](domain-events.md) para entender o que acontece depois que um
   agregado muda de estado.
6. Consulte [`business-rules.md`](business-rules.md) como referência cruzada de toda invariante,
   independentemente de a qual agregado ela pertence.

## Achado relevante (reportado, não corrigido)

Comentários XML doc em `Entities/Profile.cs` e `Entities/User.cs` (propriedade `Profile`) ainda
justificam a modelagem atual ("`Profile` embutido em `User`, não uma tabela própria") citando
"compatibilidade com persistência JSON" e "documento JSON único" — mas o pipeline JSON foi
completamente removido do repositório desde a Sprint 14.7 (ver ADR-005). O comentário está
desatualizado; a decisão de manter `Profile` como view computada sobre campos de `User` (em vez de
uma tabela própria) pode ainda ser correta hoje, mas não pelo motivo que o comentário afirma — o
motivo real, se houver, não está documentado em código. Ver [`entities.md`](entities.md) §`Profile`.
