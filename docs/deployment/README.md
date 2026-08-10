# Deployment

Documentação operacional do BeeDay — reconstruída por completo na Sprint 16.9 a partir
exclusivamente do código atual (`.github/workflows/`, `scripts/`, `src/BeeDay.Web/Program.cs`,
`web.config`, `appsettings*.json`, `src/BeeDay.Infrastructure/Configuration/`). Nenhuma afirmação
vem de `docs/history/` ou de sprints anteriores sem reverificação direta.

**Fonte da verdade:** cada documento abaixo declara individualmente as fontes exatas usadas para
validá-lo, na seção final "Fontes consultadas".

## Escopo

Como o BeeDay é construído, testado, publicado e implantado (`01-deployment.md`), como sua
configuração de runtime é montada e validada (`02-runtime-configuration.md`), o que é observável
em produção hoje (`03-observability.md`), e os procedimentos operacionais reais — backup, restore,
migrations, versionamento, manutenção (`04-operations.md`). Segurança operacional (cookies, rate
limiting, headers, CSRF) tem documento próprio em [`docs/security/02-operational-security.md`](../security/02-operational-security.md),
linkado a partir daqui onde relevante em vez de duplicado.

## Documentos

| Documento | Conteúdo |
|---|---|
| [`01-deployment.md`](01-deployment.md) | Deploy manual e automatizado, os 3 workflows do GitHub Actions (`ci.yml`, `deploy-hmg.yml`, `deploy-prd.yml`), pipeline, publicação, rollback, ambientes HMG/Produção |
| [`02-runtime-configuration.md`](02-runtime-configuration.md) | `appsettings*`, variáveis de ambiente, binding de configuração, Options, secrets, guardas de startup |
| [`03-observability.md`](03-observability.md) | Logging, Event Journal, health checks, diagnostics, ciclo de vida da aplicação |
| [`04-operations.md`](04-operations.md) | Backup, restore, recovery, migrations, versionamento, processo de release, manutenção |
| [`05-privileged-iis-control.md`](05-privileged-iis-control.md) | Boundary privilegiada de controle do IIS em HMG (STOP/START/CONFIGURE/RESTORE via SYSTEM) e a automação de promoção do script operacional (`HMG-IisControl-Updater`, Sprint 17.17) |
| [`06-cicd-pipeline-discovery-baseline.md`](06-cicd-pipeline-discovery-baseline.md) | Registro histórico congelado do baseline empírico AS-IS coletado na Sprint 19.1 (workflows, triggers, timing, deployments duplicados confirmados, Rulesets, provenance) — EPIC 19. As divergências que este documento encontrou em `01-deployment.md` (§19) foram corrigidas na Sprint 19.2; o achado de deployment duplicado em HMG (§6/§12) permanece ativo e não corrigido até a Sprint 19.6 |

## Ordem de leitura recomendada

1. `01-deployment.md` — como o binário chega ao servidor (sincronizado com a implementação atual
   na Sprint 19.2).
2. `02-runtime-configuration.md` — o que esse binário lê ao iniciar.
3. `03-observability.md` — o que dá para ver depois que ele está rodando.
4. `04-operations.md` — o que fazer quando algo dá errado.
5. `05-privileged-iis-control.md` — como o runner de baixo privilégio controla o IIS em HMG sem
   nunca virar administrador.
6. `06-cicd-pipeline-discovery-baseline.md` — o baseline real do pipeline, para quem for trabalhar
   na EPIC 19 (CI/CD Architecture, Performance & Developer Experience).

## Estado real de HMG e PRD (Sprint 18.4)

**PRD não está provisionado, por decisão arquitetural deliberada.** O único ambiente runtime real
hoje é HMG (SERV3WEB) — confirmado diretamente no servidor na Sprint 18.4. `prd` (branch Git) e
`deploy-prd.yml` são artefatos preparatórios para um provisionamento futuro em Azure, ainda não
executado contra nenhum servidor real. Ver [`02-runtime-configuration.md`](02-runtime-configuration.md)
§5 para o detalhamento completo, incluindo por que `appsettings.Production.json` não corresponde a
nenhum Runtime State existente.

## Achados relevantes (reportados, não corrigidos)

- `deploy-prd.yml`'s step "Validate deployment secrets" não inclui `BEEDAY_MIGRATOR_CONNECTION`/
  `BEEDAY_APP_CONNECTION` entre os secrets — consistente com o achado acima: produção não tem
  connection string própria nem executa migrations, pois o ambiente ainda não existe. Corrigido
  nesta mesma Sprint apenas quanto a `BEEDAY_RESEND_FROM_NAME` (já consumido pelo step seguinte,
  mas ausente do pré-check — achado original reportado em `docs/architecture/README.md`, Sprint
  16.3); os dois secrets de connection string permanecem intencionalmente ausentes até o
  provisionamento real de PRD.
- Os documentos anteriores desta pasta (`01-operations.md`, `02-backup-and-restore.md`) eram
  checklists prescritivos escritos antes da infraestrutura real (`Deploy-BeeDay.ps1`, os workflows
  de deploy) existir — movidos para [`docs/history/`](../history/README.md), substituídos pelos
  documentos acima.
