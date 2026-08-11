# Main Release Quality Gate (EPIC 19, Sprint 19.7)

**Fonte da verdade:** verificado diretamente em `.github/workflows/release-quality-gate.yml`
(novo), `ci.yml`, `validate-promotion.yml`, Rulesets via `gh api repos/.../rules/branches/main`
(reconsultado em 2026-08-11), `README.md` ("Quality gate"), `src/BeeDay.Infrastructure/Persistence/SqlServer/BeeDayDbContextFactory.cs`,
e execução real local do comando `dotnet ef migrations has-pending-model-changes` (caminho
positivo e negativo, ambos executados nesta Sprint).

**Última verificação:** 2026-08-11.

**Escopo:** criar `BeeDay — Release Quality Gate` como fronteira rigorosa e independente para
`hmg → main`, automatizar o GAP `has-pending-model-changes` identificado na Sprint 19.3. **A
ativação completa (mutação de Ruleset + remoção de `pull_request:main` de `BeeDay CI`) é
deliberadamente adiada** para depois de uma validação remota real — ver §9.

**Classificação de evidência:** `FACT`, `MEASUREMENT`, `INFERENCE`, `RECOMMENDATION`, `UNKNOWN`.

---

## 1. Before

`FACT`, reconfirmado nesta Sprint (Rulesets idênticos aos observados em 19.1-19.6):

| Check atual | Workflow produtor | Propósito | Temporário? |
|---|---|---|---|
| `BeeDay CI` | `ci.yml`, `pull_request: main` | Único check técnico de qualidade hoje para `hmg→main` — mesma suíte usada para validar PRs `sprint/*→hmg` | Sim — responsabilidade emprestada desde a Sprint 19.4 |
| `Validate Promotion` | `validate-promotion.yml` | Política pura: garante que a origem de uma PR para `main` é `hmg` (não valida qualidade técnica) | Não — responsabilidade permanente |

`main`'s Ruleset (id `20608232`) hoje exige exatamente esses dois checks, `required_approving_review_count: 0`.

## 2. Root Problem

`BeeDay CI` valida `hmg→main` com a **mesma** suíte usada para `sprint/*→hmg` — nenhuma
diferenciação de rigor entre "uma PR de feature" e "uma promoção para main", violando o princípio
já registrado no `CLAUDE.md`: *main deve receber o maior rigor de validação antes da promoção.*

## 3. Target Architecture

```
PR hmg -> main
     │
     ├──> BeeDay — Promotion Policy   (validate-promotion.yml, já existente)
     │        └── valida linhagem: head == hmg, mesmo repositório
     │
     └──> BeeDay — Release Quality Gate   (release-quality-gate.yml, novo)
              ├── Format / Static
              ├── Build (Release, --warnaserror)
              ├── Domain / Application / Infrastructure / Web / E2E Tests (+ boundary embutidos)
              ├── Validar publish
              ├── has-pending-model-changes   (GAP fechado)
              └── Bundle EF + validar
```

Ambos independentes — nenhum decide a responsabilidade do outro (§5).

---

## 4. Validation Matrix Inputs (Sprint 19.3)

`FACT`, extraído de `docs/deployment/07-validation-matrix.md` §15/§22, não reinventado:

| Validação | Categoria (19.3) | Criticidade | Duração observada | Incluída no Gate? |
|---|---|---|---|---|
| Format | STATIC | MEDIUM | ~42s | Sim |
| Build (warnaserror) | STATIC | CRITICAL | ~30s | Sim |
| Domain.Tests + boundary | FAST/EVERY PR + ARCHITECTURE | HIGH/CRITICAL | 2.6s | Sim |
| Application.Tests + boundary | FAST/EVERY PR + ARCHITECTURE | HIGH/CRITICAL | 3.2s | Sim |
| Infrastructure.Tests | INTEGRATION | HIGH | 70.4s | Sim |
| Web.Tests | INTEGRATION | HIGH | 49.1s | Sim |
| E2E.Tests | E2E | HIGH | 64.3s | **Sim — `REQUIRED` nesta fronteira**, mesmo sendo `SELECTIVE` para Fast PR (§22 já previa isso) |
| Validar publish | DEPLOYMENT VERIFICATION | MEDIUM | poucos s | Sim |
| Bundle EF + validar | STATIC/ARTIFACT | MEDIUM | ~26s | Sim |
| `has-pending-model-changes` | STATIC (GAP) | HIGH (potencial) | medido nesta Sprint, ver §6 | **Sim — GAP fechado** |
| Ferramenta de segurança (SAST/dependência) | — | — | — | `NOT CURRENTLY IMPLEMENTED` — nenhuma existe no repositório (confirmado de novo nesta Sprint, `.github/` sem CodeQL/Dependabot); não inventada |

Nenhum item foi copiado sem verificar se existe/é aplicável — a lista acima é exatamente o que
`release-quality-gate.yml` implementa, nem mais nem menos.

---

## 5. Promotion Policy × Quality Gate

`FACT` — mantidos como responsabilidades estritamente separadas, sem sobreposição de lógica:

| | `BeeDay — Promotion Policy` | `BeeDay — Release Quality Gate` |
|---|---|---|
| Pergunta | Esta PR é uma promoção válida? | Este estado tem qualidade suficiente? |
| Verifica | `head.ref`, `head.repo`, política de branches | Format, build, 5 suítes de teste, EF, publish |
| Onde roda | `pull_request: [main, prd]` | `pull_request: [main]`, com guard adicional |
| Já existia? | Sim (Sprint 18.1) | Não — novo nesta Sprint |

---

## 6. `has-pending-model-changes` — GAP fechado

`FACT` + `MEASUREMENT`, comando real determinado antes de qualquer inserção no workflow (Fase 11):

| Item | Valor |
|---|---|
| DbContext | `BeeDayDbContext` (único no repositório) |
| Startup/target project | `src/BeeDay.Infrastructure` (mesmo projeto — confirmado em `README.md`) |
| Design-time factory | `BeeDayDbContextFactory` (`IDesignTimeDbContextFactory<BeeDayDbContext>`), connection string placeholder (`Server=SERV4SQL;Database=BeeDay_Dev;...CHANGEME`), nunca conectada de fato — o comando só compara modelos compilados, não abre conexão real |
| Comando final | `dotnet ef migrations has-pending-model-changes --project src\BeeDay.Infrastructure --startup-project src\BeeDay.Infrastructure --configuration Release --no-build` |
| Requer `--no-build`? | Suportado (`--help` confirmou), reaproveita o build Release já feito no gate — sem compilação redundante |

**Caminho positivo (nenhuma mudança pendente), executado localmente:**
```
> dotnet ef migrations has-pending-model-changes --project src/BeeDay.Infrastructure --startup-project src/BeeDay.Infrastructure
No changes have been made to the model since the last migration.
Exit code: 0
```

**Caminho negativo, executado com segurança e revertido — nenhum resíduo:** adicionada
temporariamente uma `.HasComment("TEMP-19.7-NEGATIVE-PATH-TEST")` em
`HabitConfiguration.cs` (`PositiveCount`), comando reexecutado, revertido via
`git checkout -- <arquivo>` imediatamente após, `git status` confirmando árvore limpa:
```
> dotnet ef migrations has-pending-model-changes --project src/BeeDay.Infrastructure --startup-project src/BeeDay.Infrastructure
Changes have been made to the model since the last migration. Add a new migration.
Exit code: 1
```

**Contrato de falha confirmado:** `PASS` = exit 0 = "No changes..."; `FAIL` = exit 1 = "Changes have been made...". Nenhuma migration falsa foi introduzida no histórico. Nenhum `continue-on-error` usado.

## 7. EF Bundle — por que os dois continuam existindo

`FACT` — são perguntas diferentes, ambas necessárias:

| Pergunta | Validação | Resposta se `has-pending-model-changes` passar mas o bundle falhar |
|---|---|---|
| O modelo em código está totalmente representado pelas migrations já existentes? | `has-pending-model-changes` | N/A — são independentes |
| As migrations existentes conseguem ser empacotadas num executável funcional para o runner-alvo? | `Generate EF Core migration bundle` + `Validate migration bundle` | Migrations corretas no código-fonte, mas bundle corrompido/incompatível com a arquitetura do runner — cenário real que só o segundo check pega |

Nenhum dos dois foi removido em favor do outro.

---

## 8. Workflow / Script / Security / Repository Hygiene Discovery

`FACT` — nenhuma ferramenta nova inventada:

- **Workflow/script validation:** o próprio `git diff --check` + parse YAML (usados nesta Sprint,
  §13) são os mecanismos já em uso na EPIC 19; nenhuma ferramenta adicional de lint de workflow
  está configurada no repositório (`actionlint` etc. — ausência já confirmada na Sprint 19.2).
- **Repository hygiene:** nenhum check dedicado de "arquivo gerado inesperado" existe ou está
  documentado como previsto; não introduzido.
- **Security:** `NOT CURRENTLY IMPLEMENTED` — sem CodeQL/Dependabot no repositório (reconfirmado).
  Registrado como GAP, não implementado nesta Sprint (fora do princípio de não inventar
  ferramentas).

## 9. Job Structure Decision

**Single job**, mirando a estrutura já validada de `ci.yml`. A Sprint 19.5 já classificou
paralelismo de jobs como `HIGH VALUE / HIGH RISK` — nesta fronteira, o próprio Sprint prioriza
"correctness, clarity, determinism" sobre wall-clock, e `CLAUDE.md` já aceita tempo maior aqui.
Não implementado paralelismo.

## 10. Performance Reuse

`FACT` — reaproveitado da Sprint 19.5, sem redesenho: cache nativo de NuGet (`setup-dotnet`),
cache de browsers Playwright, `--no-build`/`--no-restore` nos pontos já comprovados seguros
(publish, testes, EF). Nenhum desses reduz determinismo — todos falham corretamente em cache miss.

---

## 11. Debug × Release

`FACT` + decisão: **Release only.** Nenhuma evidência de valor adicional foi encontrada para
compilar Debug neste gate — o binário publicado/implantado é sempre Release
(`Deploy-BeeDay.ps1`), e `ci.yml` já só builda Release. Compilar Debug adicionalmente só
adicionaria tempo sem detectar nenhuma classe de problema que o Release build (com
`--warnaserror`, mais rigoroso) não detecte. Não implementado.

---

## 12. Trigger Contract

`FACT` — GitHub Actions não permite filtrar `pull_request` pela branch de origem diretamente no
trigger, só pela branch de destino (`branches:`). `release-quality-gate.yml` usa
`on: pull_request: branches: [main]` + guard no job (`if: github.event.pull_request.head.ref ==
'hmg'`), complementando (não substituindo) a validação de linhagem que `Validate Promotion` já
faz de forma independente e mais completa (checa também `head.repo`, não só `head.ref`).

---

## 13. Por que a ativação completa NÃO foi executada nesta Sprint

`FACT` + decisão explícita, seguindo a própria Fase 21 desta Sprint ("Evite: Ruleset requires new
check BEFORE GitHub recognizes the new check context"):

`release-quality-gate.yml` existe apenas nesta branch — **nunca rodou remotamente**, então o check
`"Release Quality Gate"` nunca foi reportado ao GitHub. Se o Ruleset de `main` fosse atualizado
agora para exigi-lo, **qualquer PR `hmg→main`, incluindo as já abertas, ficaria bloqueada
indefinidamente** esperando um check que nunca apareceria até este código ser mesclado e uma PR
real disparar o workflow pelo menos uma vez.

Pelo mesmo motivo, **`pull_request: main` NÃO foi removido de `ci.yml` nesta Sprint** — removê-lo
agora, com o Ruleset ainda exigindo `"BeeDay CI"`, deixaria `main` **permanentemente bloqueada**
(nenhum workflow produziria mais esse check). Essa é exatamente a segunda armadilha que a Fase 21
pede para evitar ("Ruleset removes BeeDay CI BEFORE new Release Quality Gate exists").

**Sequência segura definida para ativação (não executada, aguardando autorização em etapas):**

1. Mesclar esta Sprint (`release-quality-gate.yml`) em `hmg`, depois em `main` (para que o
   workflow exista no repositório real).
2. Observar uma PR real `hmg→main` disparar `BeeDay — Release Quality Gate` com sucesso pelo
   menos uma vez — confirma que o GitHub reconhece o contexto do check.
3. **Só então**, com autorização explícita separada, atualizar o Ruleset de `main`: adicionar
   `"Release Quality Gate"` aos required checks.
4. **Só então**, com autorização explícita separada, remover `pull_request: main` de `ci.yml` e
   remover `"BeeDay CI"` dos required checks de `main`.

Nenhuma mutação de Ruleset foi solicitada, autorizada ou executada nesta Sprint.

---

## 14. Main Ruleset Transition (plano, não executado)

| Campo | Valor |
|---|---|
| Ruleset ID | `20608232` |
| Required checks atuais | `BeeDay CI`, `Validate Promotion` |
| Required checks alvo (após §13 passo 3-4) | `Release Quality Gate`, `Validate Promotion` |
| Ordem de operações | Passo 3 (adicionar) separado de passo 4 (remover) — nunca trocar atomicamente sem prova de que o novo check já reporta |
| Janela temporária aceitável | Passos 3→4 podem coexistir (`BeeDay CI` e `Release Quality Gate` ambos required) sem risco — mais rigor, não menos |
| Rollback | Reverter para `BeeDay CI` como required check, remover `Release Quality Gate`, se o novo gate se mostrar instável |

## 15. Remote Mutations

| Requested | Authorized | Executed | Result |
|---|---|---|---|
| Nenhuma | — | **Não** | Conforme §13 — ativação depende de validação remota ainda não realizada |

---

## 16. BeeDay CI Changes

`pull_request: main` **NÃO removido** — ver §13. `push: hmg` também inalterado (responsabilidade
da Sprint 19.8, conforme já registrado na 19.6).

## 17. BeeDay CI Remaining Responsibilities

`pull_request: hmg`, `pull_request: main` (temporário, ver §13), `push: hmg` (temporário, 19.8).

## 18. Rename Decision

`BeeDay CI → BeeDay — Pull Request Validation`: **DEFERRED**, não aplicado. Razão: `BeeDay CI`
ainda carrega duas responsabilidades emprestadas simultaneamente (`pull_request:main`, pendente de
ativação desta própria Sprint; `push:hmg`, pendente da 19.8) — o nome continuaria materialmente
impreciso enquanto qualquer uma das duas persistir. Reavaliação final cabe à 19.8, quando (se)
ambas estiverem resolvidas.

---

## 19. 19.4 Debt Resolution

`pull_request:main` da Sprint 19.4: **`NOT RESOLVED`** — implementação pronta
(`release-quality-gate.yml` existe e foi validado localmente), mas a substituição de proteção
requer a sequência de ativação da §13, que depende de validação remota ainda não realizada nesta
sessão. Reportado com precisão, não simplificado para fechar um checklist — mesmo padrão de
honestidade já usado nas Sprints 19.2.1/19.5.1/19.6.

## 20. 19.8 Debt (inalterada)

`push:hmg` em `BeeDay CI` continua como responsabilidade emprestada, dona futura = proveniência
final (Build Once/Deploy Many), Sprint de remoção = 19.8 — já registrado na 19.6, não alterado
aqui.

---

## 21. HMG Architecture Preservation

`FACT` — `deploy-hmg.yml`, `verify-hmg.yml` não foram tocados nesta Sprint. Nenhuma mudança em
SERV3WEB, IIS, AppPool, bindings, ou qualquer configuração de servidor — readiness/smoke
continuam sendo exclusivamente responsabilidade da 19.6.

---

## 22. Fontes consultadas (Sprint 19.7)

- `.github/workflows/release-quality-gate.yml` (novo), `ci.yml`, `validate-promotion.yml`.
- `gh api repos/tiagoarrigoni/BeeDay/rules/branches/main` (reconsultado nesta Sprint).
- `README.md` ("Quality gate"), `dotnet-tools.json`.
- `src/BeeDay.Infrastructure/Persistence/SqlServer/BeeDayDbContextFactory.cs`.
- Execução real local: `dotnet ef migrations has-pending-model-changes` (caminho positivo e
  negativo, revertido), `dotnet ef migrations has-pending-model-changes --help`.
- `docs/deployment/07-validation-matrix.md` §15/§22 (matriz de entrada, não reinventada).
- `docs/deployment/08-fast-pr-validation-decision.md` (dívida da 19.4, atualizada).
- `docs/deployment/10-hmg-deployment-verification.md` (dívida da 19.8, referenciada sem
  reescrever).
- `CLAUDE.md` §5.7 ("main possui Full Quality Gate").

---

## 23. Sprint 19.7.1 — Clean-Runner Compatibility Correction

**Fonte da verdade:** reprodução local controlada em estado limpo (2026-08-11), aplicando
diretamente a descoberta empírica da Sprint 19.5.2
(`docs/deployment/09-pipeline-performance.md` §27) a `release-quality-gate.yml`. A PR original da
Sprint 19.7 foi fechada sem merge deliberadamente — esta correção deveria existir antes de
reabri-la.

### 23.1 Achado da 19.5.2 e sua relevância aqui

`FACT`, reconfirmado (não apenas herdado por suposição): `dotnet build BeeDay.slnx --configuration
Release` continua, no estado atual do repositório, gravando os 4 projetos de `src/`
(`Domain`/`Application`/`Infrastructure`/`Web`) em `bin/Debug`/`obj/Debug`, não `bin/Release`/
`obj/Release`, apesar da flag — só os 5 projetos de `tests/` resolvem Release corretamente.
Reconfirmado por reprodução limpa nesta própria Sprint, idêntico ao observado na 19.5.2.

### 23.2 Ordem real dos steps do Release Gate (do YAML, não suposta)

```text
Checkout → Configure .NET 10 (cache NuGet) → Restore → Format → Build (Release, --warnaserror)
  → Cache/Install Playwright → Run full test suite → Publish BeeDay → Validate published files
  → Restore EF tool → Check for pending EF model changes → Generate EF Core migration bundle
  → Validate migration bundle → Upload test results → Record gate summary
```

### 23.3 Clean-State Reproduction

Método: remoção de `bin/`+`obj/` dos 4 projetos de produção antes de cada teste, seguida de
`dotnet restore` (outputs gerados, não rastreados, seguros de remover e regeneráveis). Cada step
relevante foi executado **na ordem real do gate**, isolado E encadeado, replicando exatamente os
comandos do YAML (incluindo `--configuration Release`/`--no-build` conforme cada step realmente
usa).

### 23.4 Publish Analysis

| Campo | Valor |
|---|---|
| Command (antes) | `dotnet publish .\src\BeeDay.Web\BeeDay.Web.csproj -c Release --no-restore --no-build` |
| Resultado em estado limpo | `error : Manifest file at 'obj\Release\net10.0\staticwebassets.build.json' not found.` (idêntico à 19.5.2) |
| Verdict | **`UNSAFE`** |
| Fix | `--no-build` removido — idêntico à correção já aplicada em `ci.yml` na 19.5.2 |

### 23.5 `has-pending-model-changes` Analysis

`FACT` — testado isoladamente (logo após "Build solution (Release)", sem publish antes) **e**
após o publish corrigido, nessa ordem, para não presumir nada:

| Cenário | Comando | Resultado |
|---|---|---|
| Isolado, sem publish antes | `dotnet ef migrations has-pending-model-changes --project src/BeeDay.Infrastructure --startup-project src/BeeDay.Infrastructure -c Release --no-build` | **FALHA**: `The specified deps.json [...\bin\Release\net10.0\BeeDay.Infrastructure.deps.json] does not exist` |
| Após o publish corrigido (ordem real do gate) | mesmo comando | **Sucesso**: `No changes have been made to the model since the last migration.` |

**Verdict: `SAFE ONLY AFTER PUBLISH`.** O acoplamento é real e não escondido — documentado
diretamente no comentário do step em `release-quality-gate.yml`. `--no-build` **preservado**
(não removido), porque a ordem real do gate já garante a dependência.

*Nota metodológica:* a primeira tentativa de teste desta Sprint usou barras invertidas
(`src\BeeDay.Infrastructure`) executadas via Git Bash, que interpretou `\` como caractere de
escape e corrompeu o argumento, produzindo um erro enganoso (`Unable to retrieve project
metadata`) não relacionado ao defeito real. Identificado e corrigido antes de registrar qualquer
conclusão — os resultados acima usam barras normais, equivalentes ao que o PowerShell do runner
real interpreta corretamente com barras invertidas.

### 23.6 EF Bundle Analysis

Mesma metodologia, mesmo resultado:

| Cenário | Resultado |
|---|---|
| Isolado, sem publish antes | **FALHA**: mesmo erro de `deps.json` ausente |
| Após o publish corrigido (ordem real do gate) | **Sucesso**: `Done. Migrations Bundle: ...\efbundle.exe` |

**Verdict: `SAFE ONLY AFTER PUBLISH`.** `--no-build` preservado, dependência documentada no
comentário do step.

### 23.7 Clean-Runner Compatibility Matrix

| Step | Command | Clean state (isolado) | Depende do step anterior | Verdict |
|---|---|---|---|---|
| Build | `dotnet build -c Release --warnaserror` | PASS (mas produz outputs Debug para `src/`) | — | `N/A` — dívida separada, não corrigida (§23.9) |
| Publish | `dotnet publish ... --no-build` (antes) | **FAIL** | Build (insuficiente) | `UNSAFE` → corrigido |
| Pending Model | `has-pending-model-changes --no-build` | **FAIL** isolado | Build (insuficiente) sozinho; **Publish corrigido** | `SAFE ONLY AFTER PUBLISH` → mantido |
| EF Bundle | `migrations bundle --no-build` | **FAIL** isolado | Build (insuficiente) sozinho; **Publish corrigido** | `SAFE ONLY AFTER PUBLISH` → mantido |

### 23.8 Changes Implemented / Not Implemented

**Implementado:** `--no-build` removido apenas do step `Publish BeeDay`. Comentários adicionados
aos steps `Check for pending EF model changes` e `Generate EF Core migration bundle` documentando
explicitamente a dependência de que rodem **depois** de `Publish BeeDay`.

**Não implementado:**

- `--no-build` de `has-pending-model-changes`/EF bundle **não foi removido** — comprovadamente
  seguro na ordem real do gate, removê-lo seria uma mudança desnecessária (reintroduziria
  recompilação redundante sem ganho de correção).
- `BeeDay.slnx`/`Directory.Build.props`/topologia MSBuild **não alterados** — dívida estrutural
  separada (§23.9), fora do princípio de correção mínima desta Sprint.
- Nenhuma validação removida — Format, Build, 5 suítes de teste (+ boundary embutidos), E2E,
  publish, `has-pending-model-changes`, EF bundle continuam todos presentes e `REQUIRED`.
- Nenhuma otimização nova introduzida (cache NuGet/Playwright, `--no-restore` nos testes
  preservados sem alteração).
- Ruleset de `main` **não alterado** — continua exigindo `BeeDay CI` + `Validate Promotion`.
- `pull_request: main` **não removido** de `ci.yml`.

### 23.9 BeeDay.slnx Remaining Debt (inalterada)

Mesma dívida já registrada em `09-pipeline-performance.md` §27.10 — `dotnet build BeeDay.slnx
-c Release` não propaga a configuração Release para os 4 projetos de `src/`. Reconfirmada nesta
Sprint, não corrigida, continua candidata a investigação dedicada futura.

### 23.10 Local Validation Results

| Comando | Executado | Resultado |
|---|---|---|
| `dotnet restore BeeDay.slnx` | Sim | Sucesso |
| `dotnet format BeeDay.slnx --verify-no-changes` | Sim | Sucesso |
| `dotnet build BeeDay.slnx -c Release --warnaserror` | Sim | Sucesso, 0 avisos, 0 erros |
| `dotnet test BeeDay.slnx -c Release --no-build` | Sim | 752/752 aprovados |
| Reprodução completa da sequência real do gate (9 passos, estado limpo) | Sim | Todos os 9 passos com sucesso, incluindo publish, `has-pending-model-changes` e EF bundle |
| `git diff --check` | Sim | Limpo |

### 23.11 Remaining Activation Sequence (inalterada desde a 19.7)

1. Reabrir/usar a PR da 19.7 (com esta correção).
2. Merge em `hmg`.
3. Abrir PR `hmg → main`.
4. Observar `BeeDay — Release Quality Gate` executar e passar pela primeira vez.
5. Só então, com autorização explícita separada: mutar o Ruleset de `main`.
6. Só então, com autorização explícita separada: remover `pull_request: main` de `ci.yml`.

Nenhum desses passos foi executado nesta Sprint.

---

## 24. Sprint 19.8.2 — Windows PowerShell 5.1 Compatibility

**Fonte da verdade:** auditoria própria do arquivo atual (varredura por código Python de todos os
blocos `run: |`), reprodução real com `powershell.exe` (Windows PowerShell 5.1, não `pwsh`),
`gh run view --log` de um run real de `ci.yml` (`31457503268`) para confirmar o shell resolvido em
`windows-latest`, e a evidência já registrada em `docs/deployment/12-artifact-provenance.md` §35
(Sprint 19.8.1).

### 24.1 Origem do achado

Durante a auditoria da Sprint 19.8.1 (correção de `deploy-hmg.yml`/`verify-hmg.yml` após a falha
real do run `31456637128`), uma varredura por `—` em todos os workflows encontrou o mesmo padrão em
`release-quality-gate.yml` linha 237 (step `Record gate summary`). Registrado como débito técnico
naquela Sprint, deliberadamente não corrigido ali (fronteira `hmg→main`, fora do escopo de uma
Sprint sobre `Sprint→HMG`). Esta Sprint resolve esse débito antes da primeira execução real do gate.

### 24.2 Relação com a falha remota da 19.8/evidência da 19.8.1

`FACT`: o run `31456637128` (`BeeDay — HMG Deployment`) comprovou remotamente que um em dash
(`—`, U+2014) dentro de um literal PowerShell escrito em `$env:GITHUB_STEP_SUMMARY`, executado via
`shell: powershell`, falha com `Unexpected token` no runner self-hosted (SERV3WEB) — Windows
PowerShell 5.1 decodifica o script `.ps1` temporário gerado pelo Actions runner (sem BOM) usando o
codepage legado do sistema, produzindo mojibake que inclui um caractere de aspas curvas aceito pelo
tokenizer como delimitador de string. `deploy-hmg.yml` e `verify-hmg.yml` foram corrigidos e
**validados remotamente** na Sprint 19.8.1 (deployment subsequente completou `Record deployment
info` e `Upload deployment info` com sucesso).

### 24.3 Release Quality Gate Inspection

`FACT`, confirmado por leitura direta e varredura automatizada: `release-quality-gate.yml` linha
237, step `Record gate summary` — `"## BeeDay — Release Quality Gate" >> $env:GITHUB_STEP_SUMMARY`
— mesmo padrão exato (em dash dentro de literal PowerShell, `shell: powershell`, escrevendo
`$GITHUB_STEP_SUMMARY`).

### 24.4 PowerShell Runtime Analysis

`FACT`, verificado empiricamente (não presumido por analogia): `release-quality-gate.yml` roda em
`runs-on: windows-latest` (GitHub-hosted), diferente do runner self-hosted (SERV3WEB) usado por
`deploy-hmg.yml`/`verify-hmg.yml`. Antes de aplicar a mesma correção, confirmado via
`gh run view --log` de um run real e bem-sucedido de `ci.yml` (`31457503268`, também
`windows-latest` + `shell: powershell`) que o shell resolvido é **o mesmo binário exato**:
`C:\Windows\System32\WindowsPowerShell\v1.0\powershell.EXE` (Windows PowerShell 5.1) — `shell:
powershell` sempre resolve para PS 5.1 em runners Windows, hospedados ou self-hosted; a distinção
hosted/self-hosted não é o fator relevante. Equivalência técnica confirmada, não copiada
cegamente.

### 24.5 Unicode Audit

`FACT`: varredura automatizada (script Python) de **todos** os blocos `run: |` do arquivo (14
steps com `shell: powershell`) encontrou exatamente **1** ocorrência de caractere não-ASCII —
linha 237, o em dash já identificado. Nenhum outro caractere Unicode em código PowerShell
efetivamente executado. Os em dashes nos comentários YAML (`#`, ex.: linhas 3, 7, 33) e no `name:`
do workflow (linha 1) não são código PowerShell executado — YAML/comentários são processados pelo
parser YAML do GitHub Actions, não pelo interpretador PowerShell, e não geram o arquivo `.ps1`
temporário vulnerável. Nenhuma limpeza indiscriminada de Unicode foi feita.

### 24.6 Root Cause Equivalence

| | Classificação |
|---|---|
| SYMPTOM POTENCIAL | `Record gate summary` falharia com `Unexpected token`/erros em cascata na primeira execução real, mesmo com todas as validações do gate passando |
| TECHNICAL CAUSE | Em dash em literal PowerShell → mojibake sob decodificação de codepage legado do PS 5.1 → caractere de aspas curvas aceito como delimitador de string |
| ROOT CAUSE | Caractere Unicode não-ASCII em string PowerShell executada via `shell: powershell` (PS 5.1) sem BOM no script gerado |
| EVIDENCE | Idêntica ao run `31456637128` (19.8.1) + reprodução local nova com o literal real deste arquivo (§24.7) + confirmação do mesmo binário `powershell.exe` via log real de `ci.yml` |
| FIX STRATEGY | Substituição mínima do caractere (em dash → hífen ASCII), idêntica à 19.8.1, aplicada apenas após confirmar equivalência técnica real |

### 24.7 Local Reproduction

Reproduzido com `powershell.exe` real (não `pwsh`), usando o literal exato do arquivo:

| Cenário | Comando | Resultado |
|---|---|---|
| Literal ANTES (em dash) | `powershell.exe -File old.ps1` | **FALHA**: `Token 'Release' inesperado na expressão ou instrução` — mesma classe de erro do run `31456637128` |
| Literal DEPOIS (hífen ASCII) | `powershell.exe -File fixed.ps1` | **Sucesso** (exit 0), Markdown gerado corretamente |

Sem resíduos — arquivos temporários criados em `$env:TEMP` e removidos ao final da reprodução.

### 24.8 Fix Implemented

`release-quality-gate.yml`, step `Record gate summary`: `"## BeeDay — Release Quality Gate"` →
`"## BeeDay - Release Quality Gate"` (em dash → hífen ASCII). Comentário explicativo adicionado,
citando a evidência da 19.8.1 e a confirmação do binário PS 5.1 real. Nenhuma migração para
`shell: pwsh`.

### 24.9 Why This Fix Is Minimal

Uma linha alterada (um caractere), mais comentário explicativo. Nenhuma mudança de trigger,
validação, ordem de steps, ou estratégia de artifact.

### 24.10 Release Quality Gate Preservation

`FACT`, reconfirmado por leitura completa do arquivo após a correção: todas as validações
permanecem — Format, Build (Release, `--warnaserror`), cache/install Playwright, suíte completa de
testes (5 projetos + boundary embutidos), Publish, validação do publish, restore da ferramenta EF,
`has-pending-model-changes`, EF migration bundle, validação do bundle, upload de test results.
Nenhuma removida.

### 24.11 Sprint 19.7.1 Fix Preservation

`FACT`, confirmado via `grep -n "no-build"`: `Publish BeeDay` continua **sem** `--no-build`;
`Check for pending EF model changes` e `Generate EF Core migration bundle` continuam **com**
`--no-build`, na mesma ordem (depois de Publish) — a correção da 19.7.1 está intacta, não tocada
por esta Sprint.

### 24.12 Trigger Preservation

`FACT`: `on: pull_request: branches: [main]` + `workflow_dispatch` inalterados. `if:` guard
(`workflow_dispatch` ou `head.ref == 'hmg'`) inalterado. `concurrency` inalterado.

### 24.13 Documentation Updated

Este documento (`11-release-quality-gate.md`), nova seção §24. Nenhum documento duplicado criado.

### 24.14 Local Validation Status

Ver relatório da Sprint (seção correspondente) — `dotnet format/build/test`, `git diff --check`,
YAML válido, reprodução PS 5.1 real (bug + fix).

### 24.15 Remote Validation Status

**`NOT YET VALIDATED REMOTELY`.** Este workflow nunca executou numa PR `hmg → main` real até hoje
(nenhuma PR desse tipo existiu ainda). A validação remota ocorrerá na primeira execução real,
quando a sequência de ativação do §23.11 for retomada — não nesta Sprint.
