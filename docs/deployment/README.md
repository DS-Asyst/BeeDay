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
| [`01-deployment.md`](01-deployment.md) | Deploy manual e automatizado, os 2 workflows do GitHub Actions, pipeline, publicação, rollback, ambientes HMG/Produção |
| [`02-runtime-configuration.md`](02-runtime-configuration.md) | `appsettings*`, variáveis de ambiente, binding de configuração, Options, secrets, guardas de startup |
| [`03-observability.md`](03-observability.md) | Logging, Event Journal, health checks, diagnostics, ciclo de vida da aplicação |
| [`04-operations.md`](04-operations.md) | Backup, restore, recovery, migrations, versionamento, processo de release, manutenção |

## Ordem de leitura recomendada

1. `01-deployment.md` — como o binário chega ao servidor.
2. `02-runtime-configuration.md` — o que esse binário lê ao iniciar.
3. `03-observability.md` — o que dá para ver depois que ele está rodando.
4. `04-operations.md` — o que fazer quando algo dá errado.

## Achados relevantes (reportados, não corrigidos)

- **`appsettings.Production.json` e `web.config` referenciam `C:\Apps\LevelUp-Data\...`** (3 pontos:
  `DataProtectionKeysDirectory`, `Auditing:EventJournal:Directory`, `stdoutLogFile`) **enquanto
  `scripts/Deploy-BeeDay.ps1` cria e protege ACL apenas em `C:\Apps\BeeDay-Data\...`** — não é uma
  divergência cosmética: se implantado como está hoje no repositório, o processo da aplicação
  tentaria escrever chaves de Data Protection, o Event Journal e o log de stdout em um caminho
  (`LevelUp-Data`) para o qual `IIS AppPool\BeeDayPool` nunca recebe permissão `Modify` pelo script
  de deploy. Detalhado em [`02-runtime-configuration.md`](02-runtime-configuration.md) §5 e
  [`04-operations.md`](04-operations.md) §4.
- **`deploy-prd.yml`'s step "Validate deployment secrets" não inclui `BEEDAY_RESEND_FROM_NAME`**
  na lista de 4 secrets pré-validados, embora o step seguinte ("Deploy to IIS with rollback") o
  consuma — já reportado em `docs/architecture/README.md` (Sprint 16.3); detalhado com o valor
  padrão de fallback em [`01-deployment.md`](01-deployment.md) §5.
- Os documentos anteriores desta pasta (`01-operations.md`, `02-backup-and-restore.md`) eram
  checklists prescritivos escritos antes da infraestrutura real (`Deploy-BeeDay.ps1`, os 2
  workflows) existir — movidos para [`docs/history/`](../history/README.md), substituídos pelos 4
  documentos acima.
