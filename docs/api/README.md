# API

Rascunho especulativo de uma API REST futura — não documentação de algo implementado hoje.

**Disposição final (Sprint 31.14, EPIC 31):** `beeday.v1.yaml` é o único arquivo desta pasta. O
próprio arquivo se autodeclara: `version: 1.0.0-draft`, `description: "Contrato inicial futuro. A
implementação HTTP completa não faz parte da primeira etapa."`, `servers: https://localhost:5001/api/v1`
(placeholder, nunca um host real). Nenhuma das rotas descritas (`/auth/login` retornando JSON,
`/users/me`, `/dashboard`, `/habits`, etc.) existe como API REST no BeeDay atual — a aplicação é
Blazor Server; os poucos endpoints HTTP reais (`/auth/login`, `/auth/logout`, `/culture/set`) são
formulários server-rendered, não uma API JSON, e não seguem os schemas deste arquivo. Mantido como
artefato de planejamento futuro explicitamente especulativo, não removido — mas não deve ser lido
como documentação de comportamento atual. Se uma API REST real for implementada no futuro, este
arquivo deve ser reescrito a partir da implementação verificada, não promovido como estava.

## Documentos

| Documento | Status |
|---|---|
| [`beeday.v1.yaml`](beeday.v1.yaml) | `NOT IMPLEMENTED` — rascunho especulativo, 0% do conteúdo corresponde a uma rota HTTP real do BeeDay atual. |

## Ordem de leitura recomendada

1. `beeday.v1.yaml` — apenas como referência de planejamento futuro, não como fonte de verdade
   sobre o sistema atual.
