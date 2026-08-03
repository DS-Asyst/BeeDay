# Fluxos de Execução

## 1. Login

```text
POST /auth/login
→ LoginRequest
→ validação
→ AuthenticateUserCommand
→ IUserRepository.GetByEmailAsync
→ IPasswordService.Verify
→ regras de status da conta
→ AuthenticationResponse
→ emissão de cookie com SessionVersion
```

Requisitos:

- rate limiting por IP e identidade normalizada;
- resposta genérica para credencial inválida;
- registro de tentativa sem armazenar senha;
- cookie HttpOnly, Secure em produção e SameSite definido;
- claim com `UserId` e `SessionVersion`;
- invalidação quando senha ou segurança da conta mudar.

## 2. Criar hábito

```text
SaveHabitContract
→ CreateHabitCommand
→ validator
→ current user
→ Habit.Create(...)
→ IHabitRepository.AddAsync
→ HabitResponse
```

## 3. Marcar tarefa

```text
ToggleTaskContract
→ ToggleTaskCommand
→ carregar task por UserId + TaskId
→ validar ownership
→ alterar agregado
→ publicar domain events
→ aplicar reward idempotente
→ persistir uma vez
→ retornar ActivityCompletionResponse
```

## 4. Dashboard

```text
GetDashboardQuery
→ current user
→ IDashboardReadService
→ projeção SQL Server (EfDashboardReadService — único provider de runtime desde a Sprint 14.6)
→ DashboardResponse
```

O dashboard não deve retornar entidades mutáveis nem um documento global (`LevelUpData`, removido do
Domain na Sprint 14.7 — a regra permanece como princípio arquitetural mesmo sem o tipo original que a
motivou).

## 5. Alterar senha

```text
ChangePasswordRequest
→ verificar senha atual
→ gerar novo hash
→ incrementar SessionVersion
→ revogar tokens de reset
→ persistir
→ invalidar sessões antigas na próxima validação do cookie
```

## 6. Background task

```text
handler
→ grava estado principal
→ registra outbox/audit event
→ commit
→ worker processa integração
```

Enquanto não houver Outbox, tarefas em memória devem ser classificadas como não críticas.
