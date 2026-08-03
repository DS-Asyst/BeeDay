# Persistence Architecture

**Fonte da verdade:** verificado diretamente em
`src/BeeDay.Infrastructure/Persistence/SqlServer/BeeDayDbContext.cs`, todas as classes
`*Configuration.cs` em `src/BeeDay.Infrastructure/Persistence/SqlServer/Configurations/`, os 8
repositórios e implementações, `EfUnitOfWork.cs`, e a migration
`20260803111144_InitialCreate.cs`.

## 1. `BeeDayDbContext`

`internal sealed class BeeDayDbContext(DbContextOptions<BeeDayDbContext> options) : DbContext(options)`
(`src/BeeDay.Infrastructure/Persistence/SqlServer/BeeDayDbContext.cs:7`) — `internal`, nunca
acessível fora do assembly `BeeDay.Infrastructure` exceto pelos 3 assemblies de teste com
`InternalsVisibleTo`.

### DbSets (10)

`Users`, `UserTokens`, `Habits`, `RecurringTasks`, `Projects`, `Todos`, `Wallets`, `WalletTags`,
`Transactions`, `ExperienceEntries`.

### `ConfigureConventions`

Toda propriedade `decimal` recebe `HasPrecision(19,2)`; toda propriedade `DateTimeOffset` recebe
`HasPrecision(7)` — aplicado globalmente, não por entidade.

### `OnModelCreating` — ordem funcionalmente obrigatória (comentário no código confirma)

1. `modelBuilder.Entity<Activity>().UseTpcMappingStrategy();` — mapeamento TPC (Table-Per-Concrete-Type).
2. `modelBuilder.ApplyConfigurationsFromAssembly(...)` — aplica todas as `IEntityTypeConfiguration<T>`.
   Deve rodar antes do passo 3: `OwnsOne()`/`ComplexProperty()` precisam configurar o tipo antes de
   qualquer outro código tocar esse CLR type via `modelBuilder.Entity(clrType)`, senão o EF Core
   trava o tipo como "não owned" e lança exceção.
3. Loop sobre todos os tipos de entidade concretos (excluindo abstratos, owned types,
   `ExperienceEntry` e `ExperienceSource`) adicionando uma propriedade shadow `byte[] RowVersion`
   com `.IsRowVersion()` — concorrência otimista uniforme em quase todas as tabelas.

## 2. TPC (Table-Per-Concrete-Type): `Activity`

`Activity` (abstrato, `src/BeeDay.Domain/Entities/Activity.cs`) é a base de `Habit`,
`RecurringTask`, `Project` e `Todo`. Confirmado via `UseTpcMappingStrategy()` e via
`BeeDayDbContextModelSnapshot.cs` (`b.ToTable((string)null); b.UseTpcMappingStrategy();` no tipo
base — ou seja, o tipo abstrato não tem tabela própria). Cada tipo concreto tem sua própria tabela
totalmente independente (`Habits`, `RecurringTasks`, `Projects`, `Todos`), sem tabela compartilhada
e sem coluna discriminadora. Colunas comuns (`Title`, `Description`, `Featured`, `Attribute`,
`Completed`, `CreatedAtUtc`, `UpdatedAtUtc`) são configuradas uma única vez pelo método de extensão
`ConfigureActivityProperties<TActivity>()`
(`Configurations/ActivityConfigurationExtensions.cs`), chamado pelos 4 configurations concretos.

## 3. Owned Type e Complex Type

| Tipo EF Core | Onde | Motivo |
|---|---|---|
| **Owned Type** (`OwnsOne`) | `User.Experience` (`UserExperience`) — `UserConfiguration.cs` | Mapeado para tabela própria `UserExperience`, compartilhando a PK do `User` (`WithOwner().HasForeignKey("UserId")`); tem sua própria propriedade shadow `RowVersion` explícita (o loop global de `RowVersion` pula owned types). |
| **Complex Type** (`ComplexProperty`) | `ExperienceEntry.Source` (`ExperienceSource`) — `ExperienceEntryConfiguration.cs` | Mapeado inline na própria tabela `ExperienceEntries` (colunas `SourceType`, `SourceId`, `SourceDescription`), sem tabela própria e sem identidade própria — ao contrário de Owned Type. Escolhido especificamente porque um índice único filtrado que cruza colunas próprias de `ExperienceEntry` (`UserId`, `RewardType`) com colunas do tipo aninhado (`SourceType`, `SourceId`) não pode ser expresso via nenhuma superfície Fluent API (nem `HasIndex` lambda, nem array de strings, nem `IMutableEntityType.AddIndex`) — daí a necessidade do SQL bruto abaixo. |

## 4. SQL bruto na migration (única ocorrência)

`src/BeeDay.Infrastructure/Persistence/SqlServer/Migrations/20260803111144_InitialCreate.cs`:

```csharp
// Up (linha ~339)
migrationBuilder.Sql(
    "CREATE UNIQUE INDEX [UX_ExperienceEntries_Dedup] ON [ExperienceEntries] " +
    "([UserId], [SourceType], [SourceId], [RewardType]) " +
    "WHERE [SourceId] IS NOT NULL AND [SourceType] <> 0;");

// Down (linha ~417)
migrationBuilder.Sql("DROP INDEX [UX_ExperienceEntries_Dedup] ON [ExperienceEntries];");
```

Esta é a única linha de SQL bruto em toda a migration — todo o restante usa
`CreateTable`/`CreateIndex`/`DropTable` do Fluent API do EF Core.

## 5. Migrations

Exatamente **uma** migration existe hoje:
`20260803111144_InitialCreate.cs` (+ `.Designer.cs` + `BeeDayDbContextModelSnapshot.cs`). Não há
histórico de migrations incrementais — o schema nasceu completo em um único arquivo, consistente
com a política de "banco novo, sem importação" (ver ADR-002).

## 6. Constraints e índices notáveis por tabela

| Tabela | Constraints/índices |
|---|---|
| `Users` | Unique `UX_Users_Email`; unique filtrado `UX_Users_Nickname` (`WHERE [Nickname] <> N''`); checks de `Language`/`Theme` |
| `UserTokens` | Unique `UX_UserTokens_Hash` em (`TokenHash`,`Type`); check de `Type` |
| `Habits`/`RecurringTasks`/`Projects`/`Todos` | Índice de posição (`IX_*_User_Position` ou `IX_Todos_Project_Position`) sobre a shadow property `Position`; checks de enum (`Attribute`/`Direction`/`Difficulty`/`ResetCounter`/`Repeat`) |
| `Todos` | FK para `Users` é `DeleteBehavior.NoAction` (evita múltiplos caminhos de cascade — erro SQL Server 1785) |
| `Wallets` | Unique `UX_Wallets_User` (uma wallet por usuário); FK para `Users` `NoAction` (mesmo motivo) |
| `WalletTags` | Unique `UX_WalletTags_User_Name`; check de `Color` |
| `Transactions` | Check `Amount > 0`; FK para `Wallet` CASCADE; FK para `WalletTag` `SetNull` |
| `ExperienceEntries` | Unique filtrado `UX_ExperienceEntries_Dedup` (via SQL bruto, ver acima); checks de `SourceType`/`RewardType` |

## 7. Repositórios (8) e read services (2)

| Interface (`Application`) | Implementação (`Infrastructure`) |
|---|---|
| `IUserRepository` | `EfUserRepository` |
| `IUserTokenRepository` | `EfUserTokenRepository` |
| `IHabitRepository` | `EfHabitRepository` |
| `IRecurringTaskRepository` | `EfRecurringTaskRepository` |
| `IProjectRepository` (13 métodos — o maior, inclui Todos como entidade filha) | `EfProjectRepository` |
| `IWalletRepository` (3 métodos — o menor) | `EfWalletRepository` |
| `IWalletTagRepository` | `EfWalletTagRepository` |
| `ITransactionRepository` | `EfTransactionRepository` |
| `IDashboardReadService` | `EfDashboardReadService` (todas as queries `AsNoTracking()`) |
| `IWalletReadService` | `EfWalletReadService` (idem) |

Cada `Ef*Repository` herda de `EfRepositoryBase`
(`src/BeeDay.Infrastructure/Persistence/SqlServer/Repositories/EfRepositoryBase.cs`) e tem **dois
construtores**: um público recebendo `IDbContextFactory<BeeDayDbContext>` (cria um `DbContext` por
chamada, uso standalone) e um `internal` recebendo um `BeeDayDbContext` compartilhado (usado
exclusivamente por `EfUnitOfWork`). Todos registrados `AddScoped` em
`InfrastructureServiceCollectionExtensions.cs`.

Erros de concorrência/persistência são traduzidos por `EfConcurrencySaveChanges.cs`: uma
`DbUpdateConcurrencyException` vira `ConcurrencyConflictException`; uma `DbUpdateException` vira
`PersistenceException` — ambos tipos que `Application`/`Web` podem tratar sem conhecer EF Core.

## 8. `IUnitOfWork` / `EfUnitOfWork`

`src/BeeDay.Application/Common/Contracts/IUnitOfWork.cs` expõe as 8 propriedades de repositório
mais `SaveChangesAsync`/`BeginTransactionAsync`/`CommitTransactionAsync`/`RollbackTransactionAsync`.
`EfUnitOfWork` (`src/BeeDay.Infrastructure/Persistence/SqlServer/EfUnitOfWork.cs`) cria **um único**
`DbContext` no construtor (via `IDbContextFactory.CreateDbContext()`, não assíncrono) e o
compartilha entre todos os 8 repositórios que instancia sob demanda. Registrado `AddTransient`
(não `AddScoped`) deliberadamente, para não sobreviver ao circuito inteiro do Blazor Server.
`DisposeAsync` faz rollback automático de qualquer transação ainda aberta antes de descartar o
contexto (comportamento nativo do EF Core ao descartar uma transação sem commit).

## 9. Connection string e validação no startup

`SqlServerOptions.SectionName = "BeeDay:Persistence:SqlServer"`
(`src/BeeDay.Infrastructure/Configuration/SqlServerOptions.cs`). Registrado com
`.Validate(options => !string.IsNullOrWhiteSpace(options.ConnectionString), ...).ValidateOnStart()`
em `InfrastructureServiceCollectionExtensions.cs` — a aplicação recusa iniciar sem uma connection
string configurada.

`SqlServerOptions.HealthCheckEnabled` é uma propriedade não utilizada hoje (comentário no código a
marca como obsoleta desde que o health check de SQL Server se tornou incondicional) — reportado
aqui como observação, não corrigido (fora do escopo desta Sprint).

## 10. Cobertura de teste (verificado por contagem de arquivos, não por execução nesta seção)

`tests/BeeDay.Infrastructure.Tests/Persistence/SqlServer/`: 65 testes (`[Fact]`/`[Theory]`)
cobrindo os 8 repositórios, `EfUnitOfWork`, `EfDashboardReadService`, `EfWalletReadService`.
`tests/BeeDay.Infrastructure.Tests/BeeDayDbContextTests.cs`: 11 testes adicionais, cobrindo
especificamente as decisões de mapeamento TPC/Owned Type/Complex Type contra um SQL Server
LocalDB real e descartável (nunca InMemory/SQLite).
