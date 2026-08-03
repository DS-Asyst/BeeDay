# Deployment

Ambiente operacional, observabilidade, backup/restore, IIS e o pipeline de CI/CD.

**Fonte da verdade:** verificado nesta Sprint contra `.github/workflows/ci.yml`,
`.github/workflows/deploy-prd.yml` e `scripts/Deploy-BeeDay.ps1`.

## Documentos

| Documento | Status |
|---|---|
| [`01-operations.md`](01-operations.md) | Correto — diretrizes genéricas de ambiente/observabilidade, sem nomenclatura desatualizada. |
| [`02-backup-and-restore.md`](02-backup-and-restore.md) | Parcialmente correto — diretrizes genéricas corretas, mas não cita ainda os caminhos reais introduzidos na Sprint 15.7 (`C:\Apps\BeeDay-Backups`, `C:\Apps\BeeDay-Data`). Atualização de conteúdo adiada para Sprint futura. |

## Ordem de leitura recomendada

1. `01-operations.md`
2. `02-backup-and-restore.md`
