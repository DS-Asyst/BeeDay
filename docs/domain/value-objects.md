# Value Objects

**Fonte da verdade:** verificado diretamente em cada arquivo sob `src/BeeDay.Domain/ValueObjects/`
e `src/BeeDay.Domain/Experience/ExperienceReward.cs`/`ExperienceSource.cs`.

## Padrão comum

Todos os 6 Value Objects em `ValueObjects/` são `readonly record struct` com:

- Construtor `private`, valor acessível apenas via propriedade `Value` (`string`).
- Único ponto de criação: `static Create(...)`, que normaliza a entrada e lança
  `DomainValidationException` se inválida — **nunca** existe um caminho para obter uma instância
  inválida.
- `override string ToString() => Value`.

**Imutabilidade:** garantida pela combinação `readonly record struct` + único setter sendo o
construtor privado — não há nenhum método que altere `Value` após a criação; qualquer "mudança"
exige criar uma nova instância via `Create`.

**Comparação:** herdada gratuitamente da geração de `record struct` pelo compilador — igualdade
estrutural por valor (`Value` mais o tipo), sem `Equals`/`GetHashCode` customizados em nenhum dos
6 tipos.

## `ActivityTitle`

**Arquivo:** `ValueObjects/ActivityTitle.cs`. **Uso:** título de `Habit`/`RecurringTask`/`Project`/
`Todo`, via `Activity.UpdateDetails`. **Validação:** obrigatório (não vazio após trim), máximo 100
caracteres (`MaximumLength`).

## `ActivityDescription`

**Arquivo:** `ValueObjects/ActivityDescription.cs`. **Uso:** descrição de qualquer `Activity`, via
`Activity.UpdateDetails`. **Validação:** opcional (`null`/vazio vira string vazia), máximo 500
caracteres (`MaximumLength`) — única entre os 6 sem exigência de não-vazio.

## `EmailAddress`

**Arquivo:** `ValueObjects/EmailAddress.cs`. **Uso:** `User.Email`, via `User.Create`/`UpdateAccount`.
**Validação:** obrigatório, normalizado para minúsculas, máximo 254 caracteres, formato validado
por `System.Net.Mail.MailAddress.TryCreate` (não por regex própria). O endereço parseado deve ser
igual ao input normalizado, impedindo display-name como `Alice <alice@example.com>`.

## `Nickname`

**Arquivo:** `ValueObjects/Nickname.cs`. **Uso:** `User.Nickname`, via `User.CompleteProfile`.
**Validação:** entre 3 e 20 caracteres (após trim e remoção de `@` inicial), apenas
letras/dígitos/`.`/`_`/`-`.

## `ProjectColor`

**Arquivo:** `ValueObjects/ProjectColor.cs`. **Uso:** `Project.Color`, via `Project.Update`.
**Validação:** formato `#RRGGBB` (case-insensitive na entrada, normalizado para maiúsculas);
ausente/vazio usa `Default = "#7A4FCB"` em vez de lançar.

## `UserName`

**Arquivo:** `ValueObjects/UserName.cs`. **Uso:** `User.Name`, via `User.Create`/`UpdateName`/
`UpdateAccount`. **Validação:** obrigatório, máximo 100 caracteres.

## `ExperienceReward` (Value Object, fora de `ValueObjects/`)

**Arquivo:** `Experience/ExperienceReward.cs`. `readonly record struct` com o mesmo padrão dos
demais (construtor privado, `static Create`). **Uso:** quantidade de XP a conceder, passada para
`UserExperience.Add`/`TryAdd`. **Validação:** `Amount` deve ser `> 0` — sem limite superior próprio
além do overflow checado em `UserExperience.Add`.

## `ExperienceSource`

**Arquivo:** `Experience/ExperienceSource.cs`. É um `sealed record` com igualdade estrutural, sem
identidade própria (`Entity.Id`), criado só via `static Create` e sem método de mutação. A escolha
de referência é consistente com seu uso como **Complex Type** do EF Core em
`ExperienceEntryConfiguration.cs` (ver `docs/architecture/06-persistence-architecture.md` §3).
**Uso:** identifica a origem de uma concessão de XP (`ExperienceEntry.Source`). **Validação:**
`ReferenceId`, se fornecido, não pode ser `Guid.Empty`; `Description` opcional, máximo 160
caracteres (`MaximumDescriptionLength`); `Type` deve ser um `ExperienceSourceType` válido.

**Comparação:** duas instâncias com `Type`, `ReferenceId` e `Description` iguais são iguais por
valor. A deduplicação explicita sua chave de negócio (`Type`, `ReferenceId`, `RewardType`) e não
depende da descrição.

## Fontes de verdade

**Arquivos consultados:** todos os 6 arquivos em `src/BeeDay.Domain/ValueObjects/`,
`src/BeeDay.Domain/Experience/ExperienceReward.cs`, `Experience/ExperienceSource.cs`,
`Experience/UserExperience.cs` (para o uso de `TryAdd`), `src/BeeDay.Infrastructure/Persistence/SqlServer/Configurations/ExperienceEntryConfiguration.cs`
(citado apenas para confirmar o mapeamento como Complex Type, fato já verificado na Sprint 16.3).
**Testes consultados:** `tests/BeeDay.Domain.Tests/ValueObjectTests.cs`,
`tests/BeeDay.Domain.Tests/ExperienceDomainTests.cs`.
**Entidades relacionadas:** [`user.md`](user.md), [`entities.md`](entities.md) §Activity,
[`habit.md`](habit.md), [`recurring-task.md`](recurring-task.md), [`project.md`](project.md).
