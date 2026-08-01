# Estratégia EF Core

## 1. Introdução controlada

EF Core só deve ser adicionado depois que:

- contratos públicos estiverem criados;
- `ILevelUpRepository` não for mais consumido pelos handlers novos;
- portas por agregado existirem;
- testes de contrato existirem;
- ownership estiver centralizado.

## 2. DbContext

```csharp
internal sealed class LevelUpDbContext : DbContext, IUnitOfWork
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Habit> Habits => Set<Habit>();
    public DbSet<RecurringTask> Tasks => Set<RecurringTask>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Todo> Todos => Set<Todo>();
    public DbSet<Wallet> Wallets => Set<Wallet>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
}
```

## 3. Configuração

Usar `IEntityTypeConfiguration<T>` por entidade.

Não usar Data Annotations de persistência no Domain.

## 4. Migrations

- migrations ficam em `LevelUp.Infrastructure`;
- uma migration inicial cria o schema vazio;
- aplicação não executa migrations automaticamente em produção;
- pipeline de deploy executa etapa controlada ou script idempotente;
- rollback de aplicação e rollback de schema devem ser planejados separadamente.

## 5. Concorrência

Usar `rowversion` nas entidades mutáveis. Em conflito:

- capturar `DbUpdateConcurrencyException`;
- mapear para `activity.version_conflict` ou equivalente;
- retornar 409 no HTTP;
- UI recarrega o estado e informa o usuário.

## 6. Transações

O `DbContext` atua como Unit of Work. Uma transação explícita só é necessária quando houver múltiplos commits ou integração especial. O fluxo normal usa um `SaveChangesAsync`.

## 7. Domain events

Eventos devem ser coletados dos agregados antes ou depois do commit conforme semântica:

- eventos internos que atualizam o mesmo banco: antes do commit;
- integrações externas: registrar Outbox no mesmo commit;
- publicação externa: depois do commit pelo worker.

## 8. Queries

Queries complexas podem usar projeção direta com `AsNoTracking()` para contratos de leitura. Não forçar todas as leituras por repositórios de agregados.

## 9. Testes

- testes de mapping com SQL Server real em container ou ambiente dedicado;
- não usar EF InMemory para validar comportamento relacional;
- SQLite pode apoiar testes rápidos, mas SQL Server deve existir nos testes de integração críticos.
