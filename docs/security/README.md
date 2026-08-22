# Security

Superfícies de segurança da aplicação: rate limiting de login, invalidação de sessão, e demais
proteções que não são exclusivamente de autenticação (ver [`docs/authentication/`](../authentication/README.md)
para o recorte específico de identidade).

**Fonte da verdade:** `01-security-baseline.md` foi verificado em Sprints anteriores desta migração
contra `src/BeeDay.Infrastructure/Security` e `src/BeeDay.Web/Services`;
`02-operational-security.md` foi verificado nesta Sprint (16.9) contra o mesmo código mais
`.github/workflows/`, `scripts/Deploy-BeeDay.ps1` e `appsettings.Production.json`. Ambos
reverificados e corrigidos na Sprint 31.10 (EPIC 31).

**Ownership canônico (Sprint 31.10):** `docs/security/` é o dono canônico de todo o conteúdo de
autenticação, sessão, rate limiting e proteções operacionais — incluindo o que um leitor poderia
esperar encontrar em `docs/authentication/`. `docs/authentication/README.md` é, por decisão
definitiva desta Sprint, um redirecionamento permanente e intencional para cá, não uma área
reservada aguardando conteúdo futuro — ver a nota de disposição final no próprio arquivo.

## Documentos

| Documento | Status |
|---|---|
| [`01-security-baseline.md`](01-security-baseline.md) | Correto — descreve funcionalidades reais implementadas; nomenclatura `LevelUpClaimTypes`/`LevelUp.Web` residual corrigida na Sprint 16.10 (mantidas as menções históricas legítimas: `LevelUpData` removido, valor literal da claim `levelup:session_version`). Corrigido na Sprint 31.10: referência a `AccountSidePanel.razor` (removido em 2026-08-13) e caracterização de desativação de conta como gatilho de invalidação de sessão alcançável (não é — sem Handler/UI que a acione hoje). |
| [`02-operational-security.md`](02-operational-security.md) | Correto — reconstruído na Sprint 16.9; cobre a dimensão operacional (deploy, secrets, variáveis de ambiente) de cookies, rate limiting, headers, Data Protection, password hashing, CSRF, CORS e exposição dos health endpoints. Corrigido na Sprint 31.10: §5 afirmava que nenhum middleware de headers de segurança existia — `SecurityHeadersMiddleware` foi adicionado na Sprint 30.22 e não havia sido refletido aqui. |

## Ordem de leitura recomendada

1. `01-security-baseline.md` — o que cada mecanismo faz.
2. `02-operational-security.md` — onde/como cada mecanismo é configurado e implantado.
