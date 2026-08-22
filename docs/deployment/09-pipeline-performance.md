# Pipeline Performance & Build Acceleration (EPIC 19, Sprint 19.5)

**Fonte da verdade:** verificado diretamente em `.github/workflows/ci.yml`, `Directory.Packages.props`,
`Directory.Build.props`, ausência de `global.json`/`NuGet.config`/`packages.lock.json` no
repositório, execuções reais via `gh run view --json/--log` (6 execuções de `BeeDay CI`,
2026-08-10), e medições locais controladas (`dotnet test`/`dotnet publish` com e sem flags,
2026-08-11).

**Última verificação:** 2026-08-11.

**Escopo:** medir o pipeline já estruturado pela 19.1-19.4 e acelerar trabalho **necessário**
existente, sem remover nenhuma validação. Nenhuma mudança em `deploy-hmg.yml`/`deploy-prd.yml`,
SERV3WEB, Release Quality Gate ou `has-pending-model-changes`.

**Classificação de evidência:** `FACT`, `MEASUREMENT`, `INFERENCE`, `RECOMMENDATION`, `UNKNOWN`.

---

## 1. Performance Baseline

`MEASUREMENT` — 6 execuções reais de `BeeDay CI` (`gh run view --json jobs`, `startedAt`/
`completedAt` de cada step), 2026-08-10, runs `#164` a `#170` (exceto `#166`, PR duplicada do
mesmo commit de `#165` — ver `06-...` §6):

| Métrica | Amostra (s) | Média | Min | Max | n |
|---|---|---|---|---|---|
| Checkout | 6,4,7,6,6,6 | 5.8s | 4 | 7 | 6 |
| Configure .NET 10 | 3,3,3,2,2,3 | 2.7s | 2 | 3 | 6 |
| Show .NET information | 7,7,9,8,10,17 | 9.7s | 7 | 17 | 6 |
| **Restore dependencies** | 43,78,59,53,43,55 | **55.2s** | 43 | 78 | 6 |
| Verify formatting | 42,60,39,41,35,36 | 42.2s | 35 | 60 | 6 |
| Build solution | 30,29,27,37,27,29 | 29.8s | 27 | 37 | 6 |
| Install Playwright Chromium | 19,20,19,20,18,19 | 19.2s | 18 | 20 | 6 |
| Run tests (5 projetos) | 179,206,171,178,154,172 | 176.7s | 154 | 206 | 6 |
| **Publish BeeDay** | 11,11,14,11,11,9 | **11.2s** | 9 | 14 | 6 |
| Generate EF Core migration bundle | 26,30,26,23,20,22 | 24.5s | 20 | 30 | 6 |
| **Total job (wall-clock)** | 6m26s,7m46s,6m34s,6m34s,6m22s,6m10s | **~6m35s** | 6m10s | 7m46s | 6 |

Por projeto de teste (dentro de "Run tests"):

| Projeto | Média | Min | Max | n |
|---|---|---|---|---|
| Domain (93 testes) | 2.55s | 2.25 | 2.69 | 6 |
| Application (73 testes) | 3.25s | 3.00 | 3.62 | 6 |
| Web (450 testes) | 42.5s | 35.9 | 53.8 | 6 |
| Infrastructure (129 testes) | 67.6s | 58.0 | 80.6 | 6 |
| E2E (7 testes) | 58.1s | 51.6 | 65.3 | 6 |

Estes números **refinam** (não substituem) as medições da Sprint 19.3 (n=2) com o dobro da amostra
— mesma ordem de grandeza, confirmando que a 19.3 não estava enviesada.

**Cache hit rate atual:** `N/A` — nenhum cache existia antes desta Sprint.

---

## 2. Critical Path

`FACT` — job único, sequencial, sem paralelismo interno (reconfirmado, inalterado desde a 19.1).
Caminho crítico = a soma de todas as etapas acima. "Run tests" continua dominando (~45% do
wall-clock), seguido por "Restore" (~14%) e "Verify formatting" (~11%).

---

## 3. Restore Analysis

`FACT`

| Step | Restore explícito? | Restore implícito? | Pode reaproveitar? |
|---|---|---|---|
| `Restore dependencies` (`dotnet restore BeeDay.slnx`) | Sim | — | É o único restore real |
| `Verify formatting` | Não | Não (`--no-restore` já presente) | — |
| `Build solution` | Não | Não (`--no-restore` já presente) | — |
| `Run tests` (5x `dotnet test`) | Não | **Sim, sem `--no-restore`** (antes desta Sprint) | Sim — mesmo restore de 30s atrás |
| `Publish BeeDay` | Não | Não (`--no-restore` já presente) | — |
| `Generate EF Core migration bundle` | Não | Não (`--no-build`, restore não avaliado) | — |

`MEASUREMENT` (local, 2026-08-11, cache NuGet local já quente — não representa cache miss em CI):
comparei `dotnet test <projeto> --no-build` com e sem `--no-restore` adicional, para os 5
projetos:

| Projeto | Com restore implícito | Com `--no-restore` | Δ |
|---|---|---|---|
| Domain | 2.450s | 2.116s | 0.33s |
| Application | 2.183s | 2.161s | 0.02s |
| Infrastructure | 29.174s | 28.339s | 0.84s |
| Web | 20.352s | 20.467s | -0.12s (ruído) |
| E2E | 34.710s | 36.195s | -1.49s (ruído) |

`INFERENCE`: o custo real do restore implícito repetido é **próximo do ruído de medição**
(-1.49s a +0.84s por invocação) — o restore já feito uma vez no início do job deixa o cache de
pacotes NuGet local quente, então cada `dotnet test` subsequente só faz uma checagem rápida de
"up-to-date", não um restore completo. Isso **contradiz** a suposição intuitiva de que remover 5x
o restore implícito seria um ganho grande — não é. `RECOMMENDATION`: adicionar `--no-restore` de
qualquer forma (correção técnica, risco zero, ganho pequeno mas não-negativo) — implementado nesta
Sprint por ser seguro, não por ser um ganho relevante.

---

## 4. Build Duplication Analysis

`FACT` + `MEASUREMENT` — achado mais importante desta Sprint.

| Compilação | Configuração | Classificação |
|---|---|---|
| `dotnet build BeeDay.slnx --configuration Release --warnaserror` | Release | REQUIRED |
| `dotnet test` (5x, `--no-build`) | reaproveita Release | — (não compila) |
| `dotnet ef migrations bundle` (`--no-build`) | reaproveita Release | — (não compila) |
| **`dotnet publish` (sem `--no-build`, antes desta Sprint)** | Release, mesmos inputs do build acima | **REDUNDANT — confirmado por medição** |

`MEASUREMENT` (local, 2026-08-11): `dotnet publish src/BeeDay.Web/BeeDay.Web.csproj --configuration
Release --no-restore` (comando exato usado em `ci.yml` antes desta Sprint) — **13.125s**,
reconstruindo/reavaliando os 4 projetos (`Domain`, `Application`, `Infrastructure`, `Web`) mesmo
com outputs idênticos e atualizados de um build Release feito segundos antes. O mesmo comando com
`--no-build` adicionado — **1.836s**. Confirmei que `BeeDay.Web.dll` e `web.config` continuam
presentes no output com `--no-build` (nenhuma perda de artefato).

**Δ = ~11.3s (86% de redução) só nesta etapa**, comprovado localmente, não estimado.

---

## 5. Test Execution Cost

`FACT`, ver §1/§3. Nenhuma suíte foi identificada como cara o suficiente para justificar exclusão
de "toda PR" — conclusão da 19.3 preservada. Todas as 5 execuções já usam `--no-build` desde antes
desta Sprint (nenhuma recompilação por suíte).

---

## 6. NuGet Cache Analysis

`FACT` — descoberta antes de qualquer implementação:

- Não existe `packages.lock.json` em nenhum projeto — restore não é "lock-file-determinístico",
  mas **Central Package Management** (`Directory.Packages.props`, `ManagePackageVersionsCentrally=true`)
  já fixa a versão exata de cada pacote direto, tornando o grafo de restore bem mais determinístico
  do que um repositório sem CPM (mas ainda sujeito a variação de pacotes transitivos não pinados
  explicitamente). Confirmado (Sprint 19.5.1) que o repositório também não usa
  `RestorePackagesWithLockFile`, `--locked-mode`, nem `VersionOverride` em nenhum lugar — busca
  direta em `**/*.props`/`**/*.csproj`/`**/*.targets`/`**/*.yml` não encontrou nenhuma ocorrência.
  **Central Package Management (fonte da versão de cada pacote direto) e restore lock file
  (determinismo do grafo de restore completo, incluindo transitivos) são mecanismos distintos —
  o BeeDay tem o primeiro, não o segundo.**
- Não existe `NuGet.config` — fontes padrão (`nuget.org`).
- Runner é `windows-latest` (hospedado pela GitHub, efêmero) — sem estado persistente entre runs
  sem cache explícito.
- `actions/setup-dotnet@v6.0.0` já está em uso e suporta nativamente `cache: true` +
  `cache-dependency-path`.

### 6.1 Validação do contrato oficial (Sprint 19.5.1)

`FACT`, evidência de código-fonte obtida diretamente do repositório oficial
`actions/setup-dotnet`, tag `v6.0.0` (commit `a98b56852c35b8e3190ac28c8c2271da59106c68`), via
`gh api repos/actions/setup-dotnet/contents/src/*.ts?ref=v6.0.0` — não de blog, Stack Overflow ou
inferência.

**`action.yml` (descrição do input):** *"cache-dependency-path: Used to specify the path to a
dependency file: packages.lock.json. Supports wildcards or a list of file names."*

**`README.md` oficial (comportamento documentado/exemplificado):** descreve e exemplifica a
funcionalidade **exclusivamente** em torno de `packages.lock.json`, inclusive recomendando
`dotnet restore --locked-mode` como o par natural. Afirma textualmente: *"the action searches for
NuGet Lock files (packages.lock.json) in the repository root... If lock file does not exist, this
action throws error."* Nenhum exemplo oficial usa `.csproj` ou `Directory.Packages.props`.

**`src/constants.ts`:** `lockFilePatterns = ['packages.lock.json']` — confirma que a busca
automática (quando `cache-dependency-path` **não** é informado) é restrita a esse nome de arquivo.

**`src/cache-restore.ts` (mecanismo real, decisivo):**

```ts
export const restoreCache = async (cacheDependencyPath?: string) => {
  const lockFilePath = cacheDependencyPath || (await findLockFile());
  const fileHash = await glob.hashFiles(lockFilePath);
  if (!fileHash) {
    throw new Error('Some specified paths were not resolved, unable to cache dependencies.');
  }
  ...
```

Quando `cacheDependencyPath` **é** informado (caso do BeeDay), `findLockFile()` — e portanto a
exigência de `packages.lock.json` — **nunca é chamado**. `glob.hashFiles()` é uma função genérica
de hash de conteúdo de arquivo, sem qualquer validação de que o caminho aponte para um lock file
real. O único erro possível é se o glob não casar **nenhum** arquivo — não é o caso de
`Directory.Packages.props`/`**/*.csproj`, que sempre existem no repositório. `src/cache-save.ts`
confirma que o alvo cacheado é sempre a pasta `global-packages` do NuGet
(`~/.nuget/packages`/`C:\Users\<user>\.nuget\packages\`), e que o save é pulado quando já houve
cache hit na mesma chave — nada disso depende de lock file.

**Conclusão dividida em duas camadas, deliberadamente não simplificada:**

| Camada | Veredito | Evidência |
|---|---|---|
| Mecânica/contrato de execução (funciona sem erro, cacheia e restaura corretamente) | **SUPPORTED** | Código-fonte de `cache-restore.ts`/`cache-save.ts`, tag `v6.0.0` |
| Uso oficialmente documentado/exemplificado pela action | **Não é o caso de uso documentado** — a action é descrita e exemplificada só para `packages.lock.json` | `action.yml` + `README.md`, tag `v6.0.0` |

**Decisão (Caminho A — implementação mantida):** a configuração do BeeDay é uma aplicação **válida
e segura, porém não-documentada oficialmente**, do mecanismo genérico de `cache-dependency-path`.
Isso é aceitável aqui porque a segurança exigida pela Sprint 19.5 (Fase 5) não depende de a chave
de cache ser "a forma oficialmente exemplificada" — depende de: (1) nunca falhar de forma que
quebre o pipeline, (2) sempre cair para um restore normal em caso de miss, (3) nunca mascarar um
problema de restore real. As três propriedades estão comprovadas pelo código-fonte acima,
independente de o padrão ser ou não o exemplo do README. Adotar o padrão oficial
(`packages.lock.json` + `--locked-mode`) mudaria a política de gestão de dependências do
repositório — avaliado como alternativa B1 abaixo, não adotado sem necessidade comprovada.

### 6.2 Resumo da revisão (Sprint 19.5.1)

| Campo | Valor |
|---|---|
| **Original assumption** | `cache-dependency-path: [Directory.Packages.props, **/*.csproj]` seria aceito pelo `actions/setup-dotnet@v6.0.0` como substituto válido de `packages.lock.json` |
| **Evidence reviewed** | `action.yml`, `README.md`, `src/cache-restore.ts`, `src/cache-save.ts`, `src/cache-utils.ts`, `src/constants.ts` — todos obtidos via `gh api` na tag `v6.0.0` exata (commit `a98b56852c35b8e3190ac28c8c2271da59106c68`) |
| **Finding** | A suposição está **mecanicamente correta** (código-fonte confirma que funciona sem erro e cacheia/restaura corretamente), mas **não é o uso oficialmente documentado/exemplificado** pela action (README só mostra `packages.lock.json`) |
| **Decision** | Manter a implementação da Sprint 19.5 (Caminho A) — segura pelas 3 propriedades exigidas pela Fase 5, mesmo não sendo o padrão documentado |
| **Final implementation** | Idêntica à da Sprint 19.5 — nenhuma linha alterada em `ci.yml` |
| **Remaining remote validation** | `NOT YET VALIDATED REMOTELY` — cache hit/miss reais continuam pendentes de execução no GitHub Actions |

---

## 7. Cache Strategy (implementada)

`RECOMMENDATION` implementada, com plano de segurança conforme Fase 5:

| Pergunta | Resposta |
|---|---|
| O que é cacheado? | O diretório de pacotes globais do NuGet (`cache: true` do `setup-dotnet`) |
| O que invalida? | Hash de `Directory.Packages.props` + todos os `*.csproj` (`cache-dependency-path`) |
| Cache obsoleto pode causar falso sucesso? | Não — o restore explícito (`dotnet restore BeeDay.slnx`) continua rodando sempre; o cache só acelera onde os pacotes já estão localmente, nunca substitui a validação de que o restore funcionou |
| Cache pode esconder problema de restore? | Não — se o restore falhar (ex.: versão de pacote inexistente), falha igual, com ou sem cache |
| Fallback em cache miss? | Restore completo normal — comportamento idêntico ao que existia antes desta Sprint |

O mesmo raciocínio se aplica ao cache de browsers do Playwright (`actions/cache@v4` dedicado,
caminho `~\AppData\Local\ms-playwright` **confirmado empiricamente** no log real de uma execução —
`C:\Users\runneradmin\AppData\Local\ms-playwright\chromium-1228` — chave baseada na versão pinada
de `Microsoft.Playwright` em `Directory.Packages.props`). O step `Install Playwright Chromium`
continua rodando sempre; em cache hit, a própria ferramenta `playwright.ps1 install` já é
idempotente (não rebaixa/reinstala uma revisão já presente) — `INFERENCE`, comportamento padrão
documentado do Playwright CLI, não reverificado linha a linha nesta auditoria.

---

## 8. SDK Setup Analysis

`FACT` — sem `global.json`, versão do SDK vem só de `dotnet-version: "10.0.x"` em
`actions/setup-dotnet`. `windows-latest` tipicamente já tem SDKs .NET comuns pré-instalados, então
o custo observado (~2.7s) já é baixo. **Não alterado** — remover `setup-dotnet` dependeria de
assumir que o runner sempre terá a versão certa pré-instalada, sacrificando reprodutibilidade por
um ganho de poucos segundos. Rejeitado.

## 9. Checkout Analysis

`FACT` — `actions/checkout@v7.0.1` sem `fetch-depth` explícito (default = 1, shallow clone).
Nenhum step de `ci.yml` usa histórico Git, tags, ou `git diff`. **Já é mínimo — nenhuma mudança
necessária.**

## 10. Test Parallelism Analysis

`FACT` + `INFERENCE`, classificação por par:

| Par | Classificação | Razão |
|---|---|---|
| Domain × Application | `SAFE TO PARALLELIZE` | Ambos self-contained, sem DB/rede/browser (ver `07-...` §10) |
| Infrastructure × qualquer outro | `UNSAFE TO PARALLELIZE` | `docs/testing/01-testing-strategy.md` §7 já documenta contenção real de `CREATE`/`DROP DATABASE` no LocalDB compartilhado quando a solução roda em paralelo localmente |
| Web × E2E | `POTENTIALLY SAFE — NEEDS MORE EVIDENCE` | Web usa `TestServer` (sem porta real); E2E usa Kestrel real + porta TCP — não há evidência de conflito direto, mas nenhum teste isolado foi feito nesta Sprint para confirmar |
| Infrastructure × E2E | `UNSAFE TO PARALLELIZE` | Ambos usam LocalDB real — mesmo risco documentado |

**Decisão:** não implementar paralelismo intra-job nesta Sprint. O único par seguro (Domain ×
Application) economizaria no máximo ~2.5s (o mais rápido dos dois esperaria o mais lento) — ganho
irrelevante frente à complexidade de introduzir jobs em paralelo do PowerShell (`Start-Job`/
`Start-Process`) só para essas duas suítes. `RECOMMENDATION`: não vale o risco/esforço.

## 11. Job Parallelism Analysis

`INFERENCE` — a 19.1 já confirmou 1 job só, sem paralelismo. Dividir em múltiplos jobs
(ex.: build+format num job, Infrastructure/Web/E2E em jobs paralelos separados) teoricamente
reduziria o wall-clock de "Run tests" de ~177s sequencial para ~68s (a mais lenta das três, se
rodassem em paralelo) — uma economia de wall-clock de até ~109s.

**Custo estimado por job adicional:** checkout (~6s) + setup-dotnet com cache (~3-10s) + download
do artifact de build do job anterior (não medido, mas necessário — build teria que rodar num job
e ser transferido para os outros) — plausivelmente 20-30s de overhead por job novo, **multiplicado
pelo número de jobs**, e consumindo runner-time agregado MAIOR (múltiplos runners simultâneos), não
menor, mesmo que o wall-clock percebido pelo desenvolvedor melhore.

**Decisão:** `HIGH VALUE / HIGH RISK` — benefício real de wall-clock, mas exige redesenho
estrutural (múltiplos jobs, transferência de artifact de build, novo `needs:`, mais superfície para
errar) que não pode ser feito com segurança e validado dentro desta Sprint. **Não implementado.**
Registrado como candidato para uma Sprint futura dedicada (fora da numeração restante da EPIC 19,
que já tem escopo definido — candidato a um item de otimização contínua pós-EPIC, ou a critério do
usuário antecipar).

## 12. EF Bundle Analysis

`FACT` — `dotnet ef migrations bundle --no-build` já reaproveita o build Release existente (não
restaura, não recompila — comentário já existente no arquivo confirma isso desde antes desta
Sprint). Necessidade funcional (rodar em toda PR) já decidida pela matriz 19.3/19.4. **Nenhuma
mudança de custo identificada ou necessária.**

## 13. Generated Output Analysis

`FACT` — únicos outputs regenerados desnecessariamente identificados: o rebuild implícito do
`dotnet publish` (§4, corrigido). `bin/`/`obj/` de testes não são regenerados fora do build único.
Chromium/FFmpeg do Playwright eram baixados do zero a cada run (§6/§7, agora cacheados).

## 14. Reusable Workflow Analysis

`FACT` + julgamento explícito — `deploy-hmg.yml` e `deploy-prd.yml` compartilham a forma
superficial "baixar artifact validado + chamar `Deploy-BeeDay.ps1`", mas **não têm o mesmo
comportamento**: HMG roda migrations e promove o script privilegiado de controle IIS; PRD não faz
nenhum dos dois. Isso não atende ao critério da própria Sprint ("same logic, same inputs, same
behavior") para justificar extração em workflow/action reusável. **Não implementado.** Registrado
para reavaliação quando (e se) `deploy-prd.yml` ganhar comportamento mais próximo de
`deploy-hmg.yml` — não antes da 19.6/19.8.

---

## 15. Optimization Candidate Matrix

| Candidato | Valor | Risco | Decisão |
|---|---|---|---|
| `--no-build` em `dotnet publish` | **Alto** (11.3s medidos, 86% da etapa) | **Baixo** (artefato idêntico confirmado) | **Implementado** |
| Cache NuGet nativo (`setup-dotnet`) | **Alto** (restore = maior etapa fixa, 55.2s média) | **Baixo** (fallback seguro em miss) | **Implementado** |
| Cache de browsers Playwright | Médio (19.2s média) | **Baixo** (fallback seguro, path confirmado) | **Implementado** |
| `--no-restore` nos 5 `dotnet test` | Baixo (~0-1s medidos, ruído) | **Baixo** (zero risco funcional) | **Implementado** (correção técnica, não por ganho) |
| Paralelismo Domain×Application (intra-job) | Baixo (~2.5s teórico) | Baixo | Rejeitado (esforço > ganho) |
| Paralelismo de jobs (build/test split) | Alto (~109s teórico de wall-clock) | **Alto** (redesenho estrutural, mais runner-time agregado) | Rejeitado nesta Sprint, registrado para o futuro |
| Reusable workflow entre `deploy-hmg`/`deploy-prd` | Baixo | Baixo | Rejeitado (comportamentos genuinamente diferentes) |
| Remover `setup-dotnet` | Baixo (~2.7s) | Médio (perde reprodutibilidade) | Rejeitado |
| Reduzir `fetch-depth` do checkout | Nenhum (já é 1) | — | Não aplicável |

---

## 16. Implemented Optimizations

1. `dotnet publish` ganhou `--no-build` — elimina rebuild redundante confirmado.
2. `actions/setup-dotnet` ganhou `cache: true` + `cache-dependency-path` (`Directory.Packages.props`
   + `**/*.csproj`) — cache nativo do restore NuGet.
3. Novo step `Cache Playwright browsers` (`actions/cache@v4`, caminho confirmado empiricamente,
   chave baseada na versão pinada de `Microsoft.Playwright`).
4. Os 5 `dotnet test` ganharam `--no-restore` — correção técnica de baixo/nenhum risco, ganho
   medido próximo de zero.

## 17. Rejected Optimizations

| Otimização | Motivo da rejeição |
|---|---|
| Paralelismo intra-job (Domain×Application) | Ganho (~2.5s) não justifica a complexidade |
| Paralelismo de jobs (split build/test) | `HIGH RISK` — redesenho estrutural fora do que pode ser validado com segurança nesta Sprint |
| Reusable workflow `deploy-hmg`/`deploy-prd` | Comportamentos diferentes (migrations, IIS control) — não atende ao critério "same behavior" |
| Remover `setup-dotnet` | Sacrificaria reprodutibilidade por ganho mínimo |
| E2E → `SELECTIVE` | Fora de escopo — Fase 18 desta própria Sprint proíbe revisitar essa decisão da 19.4 |

---

## 18. Before × After

`MEASUREMENT` onde local confirmado, `INFERENCE`/estimativa onde depende de execução remota:

| Métrica | Before | After (esperado) | Saving | Saving % | Fonte |
|---|---|---|---|---|---|
| `Publish BeeDay` (local) | 13.125s | 1.836s | 11.289s | 86% | `MEASUREMENT` local, 2026-08-11 |
| `Publish BeeDay` (CI, média histórica) | 11.2s | `NOT YET VALIDATED REMOTELY` | — | — | estimativa proporcional, não confirmada |
| Restore implícito em `dotnet test` (5 projetos, combinado) | ~89s (com restore) | ~89s (sem, dentro do ruído) | ~0-1s | ~0-1% | `MEASUREMENT` local |
| `Restore dependencies` (CI, cache hit) | 55.2s média | `NOT YET VALIDATED REMOTELY` | `UNKNOWN` | `UNKNOWN` | requer execução remota real |
| `Install Playwright Chromium` (CI, cache hit) | 19.2s média | `NOT YET VALIDATED REMOTELY` | `UNKNOWN` | `UNKNOWN` | requer execução remota real |

**Nenhum ganho de cache foi inventado ou estimado como fato** — apenas o ganho de `--no-build` no
publish tem medição local direta e comparável ao comando real usado em CI.

## 19. Wall-clock Comparison

`MEASUREMENT` parcial: ~11s de wall-clock local confirmados (publish). Total de wall-clock em CI
só será conhecido após execução remota (§24).

## 20. Runner-time Comparison

Idêntico ao wall-clock nesta Sprint — job único, sem paralelismo, então runner-time consumido =
wall-clock do job (mesma relação documentada na 19.1).

## 21. Cache Cold/Warm Comparison

`NOT YET VALIDATED REMOTELY` — nenhuma execução remota com os caches novos ocorreu ainda. Não é
possível comparar cold/warm sem pelo menos 2 execuções reais (a primeira sempre será cold/miss por
definição, já que a chave de cache não existe ainda no repositório).

---

## 22. Coverage Preservation

`FACT` — as mesmas 5 suítes (752 testes), mesmos 2 boundary tests, mesmo Format, mesmo Build
`--warnaserror`, mesmo bundle EF + validação continuam rodando, na mesma ordem, com o mesmo
critério de falha. Nenhum projeto de teste, nenhuma suíte, nenhuma validação foi removida,
tornada opcional, ou marcada `continue-on-error`. `git diff` confirma que `git diff --check` está
limpo e nenhuma linha de trigger/condition/permission foi tocada.

## 23. Failure Semantics

`FACT` — confirmado por leitura direta do diff: a lógica de detecção de falha
(`$LASTEXITCODE -ne 0` → `$anyFailed = $true` → `throw`) não foi tocada, apenas ganhou uma flag
adicional na linha de comando. `dotnet format --verify-no-changes`, `dotnet build --warnaserror`,
e a validação do bundle de migração continuam com seus mecanismos de falha originais, intocados.

---

## 24. Remote Validation Status

`NOT YET VALIDATED REMOTELY` — nenhum push/PR com estas mudanças foi autorizado ainda.
**Reason:** cache hit/miss, tempo real de restore/Playwright com cache, e o wall-clock total do
job só podem ser confirmados com execução real no GitHub Actions (a primeira execução após o
merge será necessariamente um cache miss/cold run; a segunda em diante mostrará o efeito real).
**Expected validation after:** push desta branch + pelo menos 2 execuções reais de `BeeDay CI`
(uma fria, uma quente).

---

## 25. HMG / Release Gate — explicitamente adiados

Nenhuma mudança em `deploy-hmg.yml`, `deploy-prd.yml`, SERV3WEB. `has-pending-model-changes`
continua fora do pipeline, reservada para a Sprint 19.7.

---

## 26. Fontes consultadas (Sprints 19.5)

- `.github/workflows/ci.yml` (antes e depois desta Sprint).
- `Directory.Packages.props`, `Directory.Build.props`, ausência de `global.json`/`NuGet.config`/
  `packages.lock.json` (confirmada por busca direta).
- `gh run view --json jobs` / `--log` para as runs `31377545172` (#164), `31378253170` (#165),
  `31437006232` (#167), `31437586422` (#168), `31438532464` (#169), `31439181319` (#170).
- Medições locais: `dotnet test` (5 projetos, com/sem `--no-restore`), `dotnet publish` (com/sem
  `--no-build`), 2026-08-11.
- `docs/testing/01-testing-strategy.md` §7 (contenção de LocalDB já documentada).
- `docs/deployment/06-cicd-pipeline-discovery-baseline.md`, `07-validation-matrix.md`,
  `08-fast-pr-validation-decision.md`.

---

## 27. Sprint 19.5.2 — Remote CI Validation

**Fonte da verdade:** `gh pr checks`, `gh run view --json/--log` sobre a execução real falha
(`31443485421`) e uma execução anterior bem-sucedida (`31378253170`, #165, pré-19.5) da PR #55;
reprodução local controlada em estado limpo (2026-08-11).

**Contexto:** a Sprint 19.5 passou em todas as validações locais, mas a execução real do
`BeeDay CI` no GitHub Actions, disparada pela publicação da branch, **falhou**. Esta seção
documenta a investigação, causa-raiz e correção da Sprint 19.5.2.

### 27.1 Falha remota original

| Campo | Valor |
|---|---|
| PR | #55 (`sprint/19.5-pipeline-performance` → `hmg`) |
| Workflow | `BeeDay CI` |
| Run | `31443485421` |
| Job | `BeeDay CI` |
| Step | `Publish BeeDay` (step 11 de 19) |
| Comando | `dotnet publish .\src\BeeDay.Web\BeeDay.Web.csproj --configuration Release --no-restore --no-build --output ...` |
| Erro | `Manifest file at 'obj\Release\net10.0\staticwebassets.build.json' not found.` (`Microsoft.NET.Sdk.StaticWebAssets.References.targets(16,5)`) |
| Exit code | 1 |
| Steps anteriores | Todos com sucesso, incluindo Restore, Format, Build, cache Playwright, Install Playwright, **Run tests (752/752)** |
| Steps posteriores | Todos `skipped` (Validate published files, Restore EF tool, Generate/Validate EF bundle, upload de publish/migrations) |

Achado colateral, não relacionado: `gh pr checks 55` também reportou `Validate Promotion` como
`fail` — investigado e confirmado **não pertencer à Sprint 19.5**: é o resultado (correto,
esperado) da PR #54, já fechada, que havia sido aberta por engano diretamente contra `main` em vez
de `hmg` a partir da mesma branch. `Validate Promotion` rejeitou corretamente essa PR
(`Invalid promotion path 'sprint/19.5-pipeline-performance -> main'`), cumprindo exatamente sua
função. Fora de escopo desta Sprint.

### 27.2 Failure Reproduction Matrix

| Environment | Command/Step | Result | Evidence |
|---|---|---|---|
| Local (estado com resíduos de sessões anteriores) | `dotnet publish ... --no-build` | PASS (falso positivo) | Relatório original da Sprint 19.5 |
| GitHub Actions (`31443485421`) | `dotnet publish ... --no-build` | **FAIL** | Log real, exit code 1 |
| Local, estado limpo (obj/bin removidos para os 4 projetos `src/`) | `dotnet publish ... --no-build` | **FAIL, erro idêntico** | Reproduzido nesta Sprint |
| Local, estado limpo | `dotnet publish ...` (sem `--no-build`, corrigido) | PASS | Reproduzido nesta Sprint, `exit 0` |
| GitHub Actions, após correção | `dotnet publish ...` (corrigido) | Ver §27.9 | `gh run view` pós-push |

### 27.3 Root Cause

```
SYMPTOM
  dotnet publish --no-build falha no GitHub Actions com
  "Manifest file at 'obj\Release\net10.0\staticwebassets.build.json' not found."
↓
IMMEDIATE FAILURE
  O manifesto de Static Web Assets em configuração Release para BeeDay.Web não existe
  no momento em que o publish (com --no-build) o procura.
↓
TECHNICAL CAUSE
  `dotnet build BeeDay.slnx --configuration Release` NÃO builda os 4 projetos de
  src/ (Domain, Application, Infrastructure, Web) em configuração Release — grava
  os outputs em bin/Debug + obj/Debug apesar da flag --configuration Release.
  Confirmado IDENTICAMENTE no log real da execução falha (`31443485421`) e em
  reprodução local limpa — não é diferença de ambiente. Os 5 projetos de teste,
  em contraste, resolvem Release corretamente na mesma invocação.
↓
ROOT CAUSE
  Comportamento pré-existente (não introduzido pela Sprint 19.5) do build via
  `BeeDay.slnx` — o novo formato de solução XML não tem seção explícita de
  mapeamento de configuração por projeto (diferente do GlobalSection
  ProjectConfigurationPlatforms do .sln legado). Isso sempre existiu, mas era
  invisível: `dotnet publish` sem `--no-build` sempre fazia seu próprio rebuild
  implícito de todo o grafo de dependências (Domain→Application→Infrastructure→Web),
  que — como um build de projeto único, sem a ambiguidade do build de solução —
  sempre resolveu Release corretamente. Esse rebuild implícito produzia, como
  efeito colateral, TODOS os outputs Release corretos, incluindo o manifesto de
  Static Web Assets E o build Release de BeeDay.Infrastructure, do qual o step
  seguinte (`dotnet ef migrations bundle --configuration Release --no-build`)
  sempre dependeu implicitamente, sem que isso fosse percebido. A Sprint 19.5
  removeu esse efeito colateral ao adicionar `--no-build` ao publish, expondo
  pela primeira vez um defeito que já existia.
```

**Evidência decisiva** (reprodução local limpa, `git checkout`-reversível, outputs regenerados):

```
$ rm -rf src/BeeDay.{Domain,Application,Infrastructure,Web}/{bin,obj}
$ dotnet build BeeDay.slnx --configuration Release --warnaserror
  BeeDay.Web -> ...\src\BeeDay.Web\bin\Debug\net10.0\BeeDay.Web.dll     ← Debug, apesar de -c Release
  [...]
  0 Aviso(s)  0 Erro(s)

$ dotnet publish .\src\BeeDay.Web\BeeDay.Web.csproj -c Release --no-restore --no-build
error : Manifest file at 'obj\Release\net10.0\staticwebassets.build.json' not found.   ← reproduzido

$ dotnet build src/BeeDay.Web/BeeDay.Web.csproj --configuration Release --no-restore
  BeeDay.Web -> ...\src\BeeDay.Web\bin\Release\net10.0\BeeDay.Web.dll   ← Release correto, projeto direto
```

O mesmo padrão (`bin/Debug` para os 4 projetos de `src/`, `bin/Release` para os 5 de `tests/`) foi
confirmado linha a linha no log da própria execução `31443485421` que falhou no GitHub Actions —
não é uma diferença local × CI, é o mesmo comportamento nos dois ambientes.

### 27.4 Local × GitHub Runner Difference

`FACT`: **não há diferença de ambiente relevante** — o defeito é idêntico local e remotamente
(mesma versão de SDK, `10.0.302`, confirmada nos dois lados). A única diferença real foi que o
workspace local desta sessão continha `obj/Release/net10.0/staticwebassets.build.json`
**residual**, gerado por execuções anteriores de `dotnet publish` completas (sem `--no-build`)
feitas durante a própria auditoria da Sprint 19.5 — um falso positivo local clássico, exatamente
como o prompt desta Sprint antecipou.

### 27.5 Clean-State Investigation

Método usado (reversível, documentado): remoção de `bin/`+`obj/` dos 4 projetos de produção
(`src/BeeDay.Domain`, `BeeDay.Application`, `BeeDay.Infrastructure`, `BeeDay.Web`) — diretórios de
output gerados, não rastreados pelo Git, seguros de remover e idempotentemente regeneráveis via
`dotnet restore`/`build`. Nenhum arquivo rastreado ou não-rastreado do usuário foi tocado.

### 27.6 Verdicts

| # | Otimização | Veredito |
|---|---|---|
| 8 | `dotnet publish --no-build` | **UNSAFE** — falha comprovada em runner limpo (local e remoto). Corrigido: `--no-build` removido. |
| 9 | Cache NuGet (`setup-dotnet`) | **SAFE, CONFIRMED REMOTELY** — reportou `Dotnet cache is not found` (miss esperado, primeira execução), caiu corretamente para restore normal, que teve sucesso. |
| 10 | Cache de browsers Playwright | **SAFE, CONFIRMED REMOTELY** — reportou `Cache not found for input keys: ...` (miss esperado), instalação normal teve sucesso. |
| 11 | `dotnet test --no-restore` | **SAFE, CONFIRMED REMOTELY** — as 5 invocações de teste completaram com sucesso (752/752) antes mesmo do publish falhar. |

### 27.7 Fix Implemented

Removida a flag `--no-build` do step `Publish BeeDay` em `ci.yml`. Nenhuma outra linha do step foi
alterada. `--no-restore` foi mantido (não relacionado ao defeito — o restore em si nunca foi o
problema).

### 27.8 Minimality Analysis

Não foi necessário alterar o step `Generate EF Core migration bundle` (que também usa
`--configuration Release --no-build`), apesar de depender do mesmo tipo de output Release de
`BeeDay.Infrastructure`: a ordem dos steps em `ci.yml` coloca `Publish BeeDay` **antes** de
`Generate EF Core migration bundle` — o rebuild implícito do publish corrigido já produz o output
Release de `BeeDay.Infrastructure` como efeito colateral, exatamente como sempre fez antes da
Sprint 19.5 (confirmado por reprodução local: `dotnet ef migrations bundle --no-build` teve
sucesso imediatamente após o publish corrigido, em estado limpo). Alterar o step de EF bundle
também teria sido uma mudança desnecessária — não implementada.

Não foi feita nenhuma tentativa de corrigir a causa raiz mais profunda (o build de solução não
resolver Release para os 4 projetos de `src/`) — isso exigiria investigar/alterar o formato
`.slnx` ou a forma como `dotnet build` é invocado no nível de solução em toda a EPIC 19, uma
mudança de escopo muito maior que uma correção mínima de CI. Registrado como débito explícito
(§27.10), não escondido.

### 27.9 Performance Impact

| Optimization | 19.5 | 19.5.2 | Reason |
|---|---|---|---|
| NuGet cache | enabled | enabled | Validated remotely (miss → fallback OK) |
| Playwright cache | enabled | enabled | Validated remotely (miss → fallback OK) |
| `dotnet test --no-restore` | enabled | enabled | Validated remotely (752/752 passed) |
| `dotnet publish --no-build` | enabled | **removed** | Clean-runner incompatibility (confirmado local + remoto) |

**Regressão de performance assumida:** o ganho de ~11.3s medido na Sprint 19.5 para o step
`Publish BeeDay` (13.1s → 1.8s local) **não se realiza** — o step volta ao comportamento anterior
(~11s, rebuild completo do grafo de dependências). Isso é reportado explicitamente, não escondido.
Os demais ganhos (cache de NuGet/Playwright, quando houver cache hit; `--no-restore` nos testes)
permanecem válidos.

### 27.10 Remaining Debt (novo, descoberto nesta Sprint)

`dotnet build BeeDay.slnx --configuration Release` builda os 4 projetos de `src/` em Debug, não
Release — defeito **pré-existente**, não introduzido por nenhuma Sprint da EPIC 19, presente desde
antes da Sprint 19.1. Permaneceu invisível porque nada dependia diretamente dos outputs Release da
solução até a Sprint 19.5 introduzir `--no-build` no publish. Não corrigido nesta Sprint (fora do
princípio de correção mínima). Recomendação: investigação dedicada de por que `BeeDay.slnx` não
propaga `--configuration Release` para os 4 projetos de `src/` (mas propaga corretamente para os 5
de `tests/`) — candidata a uma Sprint futura de correção de build, não necessariamente dentro do
escopo restante da EPIC 19.

**Resolvido em 2026-08-22** (bloqueio real de promoção `hmg → main`, PR #316): causa raiz isolada
por reprodução determinística, incrementando o `.slnx` projeto a projeto e pasta a pasta — não é
`src/` em si, é qualquer projeto hospedado em uma pasta lógica aninhada em dois ou mais níveis
(`/src/` → `/src/Core/` → projeto). Pastas de nível único (`/tests/`, ou `/src/` sem subpastas)
sempre resolveram `Release` corretamente; a suposição registrada aqui ("dívida de `src/` vs.
`tests/`") estava parcialmente certa quanto ao sintoma, mas a causa real era profundidade de
aninhamento, não a pasta em si. Corrigido achatando `/src/Core/`, `/src/Infrastructure/` e
`/src/Presentation/` em um único `/src/` de nível único, igual a `/tests/` — ver
`docs/architecture/02-solution-structure.md` §1 para a evidência completa e
`docs/deployment/11-release-quality-gate.md` §23.9 para a atualização do achado espelhado da Sprint
19.7.1.

### 27.11 Local Validation Results (pós-correção)

| Comando | Executado | Resultado |
|---|---|---|
| `dotnet restore BeeDay.slnx` | Sim | Sucesso |
| `dotnet format BeeDay.slnx --verify-no-changes` | Sim | Sucesso |
| `dotnet build BeeDay.slnx -c Release --warnaserror` | Sim | Sucesso, 0 avisos, 0 erros |
| `dotnet test BeeDay.slnx -c Release --no-build` | Sim | 752/752 aprovados |
| `dotnet publish ... ` (corrigido, estado limpo) | Sim | Sucesso, `exit 0` |
| `dotnet ef migrations bundle --no-build` (estado limpo, pós-publish) | Sim | Sucesso |
| `git diff --check` | Sim | Limpo |

### 27.12 Remote Validation Results

Ver §27.13 abaixo — preenchido após push autorizado e observação real do GitHub Actions.

---

## 28. Sprint 19.8.5 — Fast HMG Developer Feedback

**Escopo:** reduzir o tempo de `BeeDay CI` (fronteira `Sprint→HMG`) movendo Format,
Infrastructure.Tests, Web.Tests, E2E.Tests (+ setup do Playwright) para `Release Quality Gate`,
que já os executa obrigatoriamente antes de `main`. Decisão completa, com evidência e
justificativa por item: [`08-fast-pr-validation-decision.md`](08-fast-pr-validation-decision.md)
§12.

### 28.1 Before (MEASURED REMOTELY)

5 execuções reais recentes de `BeeDay CI` (`pull_request`→`hmg`, `windows-latest`):

| Métrica | Valor |
|---|---|
| Média | 6m23s |
| Mediana | 6m22s |
| Mínimo | 5m00s |
| Máximo | 7m39s |

### 28.2 Local Fast-Gate Simulation (MEASURED LOCALLY)

Ver relatório da Sprint (seção correspondente) para a execução local passo a passo dos comandos
que permanecem no novo `ci.yml`, na mesma ordem, medindo `Restore`/`Build`/`Domain+Application
Tests`/`Publish`/`EF bundle` individualmente. Simulação local não prova comportamento em runner
limpo (`windows-latest`) — apenas a corretude da sequência e dos exit codes.

### 28.3 Expected After (ESTIMATED, não medido remotamente ainda)

Soma dos custos médios remotos reais dos itens que **permanecem** (Checkout, Configure .NET +
Restore ≈48s combinados, Build ≈28s, Domain+Application ≈5.6s, Publish ≈11s, EF tool restore + EF
bundle ≈19s, uploads ≈10s, `Show .NET information` ≈9s, mantido fora de escopo) menos os itens
**removidos** (Format ≈42s, Cache+Install Playwright ≈10-21s, Infrastructure.Tests ≈81.3s,
Web.Tests ≈44.9s, E2E.Tests ≈53.9s):

**Estimativa: ≈2m10s-2m20s** (`ESTIMATED` — projeção a partir de médias remotas reais dos steps
mantidos, não uma nova execução remota do gate reduzido). Redução absoluta estimada: **≈4 minutos
(~64%)** frente à média Before de 6m23s. Não tratar como resultado real até uma execução remota
real confirmar — ver §32 do relatório da Sprint para o status.

### 28.4 Classificação de evidência

| Medição | Classificação |
|---|---|
| Before (6m23s média) | `MEASURED REMOTELY` |
| Simulação do novo gate | `MEASURED LOCALLY` |
| Redução esperada (~64%) | `ESTIMATED` |
| Redução real após merge | `MEASURED REMOTELY` |

**Atualização — validado remotamente (PR #69, run `31490432167`):** `BeeDay CI` reduzido rodou em
**2m07s**, contra a média baseline de 6m23s — economia absoluta de **4m16s**, redução real de
**66.8%** (vs. média e vs. mediana), superando a estimativa de ~64%. `beeday-publish`/
`beeday-migrations` produzidos corretamente; `BeeDay CI` pós-merge continuou `ELIMINATED`;
`BeeDay — HMG Deployment` e `BeeDay — HMG Verification` ambos `SUCCESS`. `REMOTE VALIDATED`.

### 28.5 Sprint 19.8.6 — Rename (identidade, não performance)

`ci.yml` renomeado para `BeeDay — Pull Request Validation` (job `Pull Request Validation`) — ver
[`08-fast-pr-validation-decision.md`](08-fast-pr-validation-decision.md) §14. Nenhuma mudança de
conteúdo do gate; duração esperada permanece na mesma faixa (~2m). Ver relatório da Sprint para a
duração real medida após o rename.
