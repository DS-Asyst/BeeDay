# Security

Superfícies de segurança da aplicação: rate limiting de login, invalidação de sessão, e demais
proteções que não são exclusivamente de autenticação (ver [`docs/authentication/`](../authentication/README.md)
para o recorte específico de identidade).

**Fonte da verdade:** verificado nas Sprints anteriores desta migração contra
`src/BeeDay.Infrastructure/Security` e `src/BeeDay.Web/Services`.

## Documentos

| Documento | Status |
|---|---|
| [`01-security-baseline.md`](01-security-baseline.md) | Correto — descreve funcionalidades reais implementadas; nomenclatura "LevelUp" residual pendente de atualização. |

## Ordem de leitura recomendada

1. `01-security-baseline.md`
