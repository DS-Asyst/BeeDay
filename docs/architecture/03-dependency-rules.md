# Regras de Dependência

## 1. Dependências permitidas

```text
LevelUp.Domain       → nenhuma camada interna
LevelUp.Contracts    → nenhuma camada interna
LevelUp.Application  → Domain + Contracts
LevelUp.Infrastructure → Application + Domain + Contracts
LevelUp.Web          → Application + Contracts + Infrastructure (composition root)
Tests                → projeto sob teste + dependências de teste
```

## 2. Dependências proibidas

- Domain → EF Core, ASP.NET Core, MediatR ou Contracts.
- Contracts → Domain, Application, Infrastructure ou Web.
- Application → `DbContext`, migrations, JSON serializer ou componentes Razor.
- Web → tipos concretos de repositório.
- Handlers → `LevelUpData`.
- Contratos públicos → entidades de domínio.
- Persistence entities → componentes de UI.

## 3. Regra de modelos

Cada fronteira possui seu próprio modelo:

```text
Contract DTO ≠ Command ≠ Domain Entity ≠ EF Persistence Model ≠ View State
```

Mapeamentos explícitos são obrigatórios. Mapeamento implícito por serialização não é permitido.

## 4. Regra de ownership

Todo acesso a dados pertencentes ao usuário deve receber `UserId` da identidade autenticada. O cliente não pode escolher livremente o proprietário do recurso.

Correto:

```csharp
var userId = currentUser.RequireUserId();
await repository.GetHabitAsync(userId, habitId, ct);
```

Proibido:

```csharp
await repository.GetHabitAsync(request.UserId, request.HabitId, ct);
```

para operações do usuário autenticado.

## 5. Regra de transação

- Um command executa em uma unidade de trabalho.
- Um handler não deve chamar `SaveChangesAsync` mais de uma vez, salvo caso documentado.
- E-mail, telemetria e outras integrações externas não devem participar diretamente da transação SQL.
- Eventos externos confiáveis devem usar Outbox quando se tornarem críticos.

## 6. Regra de versionamento

- Mudança aditiva compatível: mesma versão.
- Campo renomeado, removido ou semântica alterada: nova versão.
- Erros possuem código estável.
- Enum público não pode ser reordenado caso seja serializado numericamente; preferir strings.
