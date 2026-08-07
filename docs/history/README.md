# History

Registro histórico congelado no tempo: diários de sprint, decisões de migração já concluídas e
transições encerradas. **Nada nesta pasta é referência de estado atual** — cada documento descreve
o sistema como ele era no momento em que foi escrito, não como é hoje.

**Fonte da verdade:** cada documento é, por definição, um relato do estado do código em uma data
passada específica (indicada no próprio texto ou inferível pela sprint citada) — não foi
reverificado contra o código atual e não deve ser tratado como se tivesse sido.

## Documentos

| Documento | Origem | Conteúdo |
|---|---|---|
| [`current-state-sprint-log.md`](current-state-sprint-log.md) | ex `architecture/01-current-state.md` | Autodeclarado histórico; apontava para o antigo `08-migration-status.md` como fonte mais precisa. |
| [`target-architecture-sprint-log.md`](target-architecture-sprint-log.md) | ex `architecture/02-target-architecture.md` | Arquitetura alvo que incluía um projeto `Contracts` separado — nunca implementado. |
| [`domain-aggregate-map.md`](domain-aggregate-map.md) | ex `architecture/05-domain-aggregate-map.md` | Mapa de Aggregates da Sprint 13.1. |
| [`domain-persistence-map.md`](domain-persistence-map.md) | ex `architecture/06-domain-persistence-map.md` | Mapeamento Domain↔Persistência da Sprint 13.2. |
| [`persistence-contracts.md`](persistence-contracts.md) | ex `architecture/07-persistence-contracts.md` | Diário de definição de contratos de persistência, Sprints 13.3–14.6. |
| [`migration-status.md`](migration-status.md) | ex `architecture/08-migration-status.md` | Estado do código verificado ao final da migração JSON→SQL Server (Sprint 14.7). |
| [`json-to-sql-transition.md`](json-to-sql-transition.md) | ex `data/03-json-to-sql-transition.md` | Estratégia de transição JSON→SQL Server, passos A–G, hoje concluída. |
| [`hmg-production-observability-planning.md`](hmg-production-observability-planning.md) | ex `deployment/01-operations.md` | Checklist prescritivo de ambientes/observabilidade escrito antes da Sprint 16.9 verificar `.github/workflows/`/`scripts/`/`Program.cs` reais — substituído por `deployment/01-deployment.md` e `deployment/03-observability.md`. |
| [`backup-restore-planning.md`](backup-restore-planning.md) | ex `deployment/02-backup-and-restore.md` | Checklist prescritivo de backup/restore escrito antes de `Deploy-BeeDay.ps1` existir — substituído por `deployment/04-operations.md`. |

## Ordem de leitura recomendada

Não há ordem de leitura recomendada — são registros independentes; consulte apenas o que for
relevante para entender o histórico de uma decisão específica.
