# Domain Persistence Map (Sprint 13.2)

**Status:** validado — mapeamento arquitetural concluído, sem alterações de comportamento nesta Sprint.
**Escopo:** decisões de persistência derivadas exclusivamente do Domain e do
[Aggregate Map (Sprint 13.1)](05-domain-aggregate-map.md), que permanece a referência oficial e não foi
reavaliado.

**Princípio obrigatório:** nenhuma decisão registrada aqui depende de SQL Server, PostgreSQL, SQLite,
MongoDB, Cosmos DB, EF Core ou JSON. Qualquer decisão que só fizesse sentido sob uma tecnologia
específica foi tratada como incorreta e descartada. Onde os documentos técnicos existentes
(`docs/data/01-relational-model.md`, `docs/data/02-ef-core-strategy.md`) já assumem SQL Server, isso é
esperado — são documentos de uma camada de decisão posterior (qual banco) — mas não foram usados como
insumo para as decisões deste documento, que respondem a uma pergunta anterior (como o Domain precisa
ser tratado, independentemente de banco).

Vocabulário usado neste documento, deliberadamente agnóstico de tecnologia:

| Termo aqui | Não usar (amarra a uma tecnologia) |
|---|---|
| referência por identificador | foreign key / FK |
| unicidade garantida pela camada de persistência | índice único |
| token de concorrência otimista | `rowversion` |
| adapter de persistência | `DbContext` / arquivo JSON |
| carga completa do agregado | "SELECT com JOIN" / "documento completo" |

## 0. Divergência encontrada e reportada (não corrigida nesta Sprint)

`docs/data/01-relational-model.md` foi escrito antes da Sprint 13.1 e diverge do Aggregate Map validado
em dois pontos:

1. **`WalletTags`** é modelado lá com `WalletId` como referência e unicidade `(WalletId, NormalizedName)`.
   O Aggregate Map (13.1, §2.8) confirmou no código que `WalletTag.UserId` existe e `WalletTag` **não
   tem** `WalletId` — a tag pertence ao usuário, não a uma carteira. O desenho relacional existente
   precisará ser corrigido para `(UserId, NormalizedName)` quando a Sprint que trata de SQL Server for
   executada.
2. **`Todos`** é modelado lá como tabela plana com `ProjectId` **anulável**. O Aggregate Map (13.1,
   §2.5) confirmou que `Todo` é uma entidade filha de `Project` com ciclo de vida genuinamente amarrado
   (a exclusão de `Project` depende disso) — `ProjectId` não deveria ser anulável nem `Todo` deveria ser
   endereçável sem seu `Project`.

Por CLAUDE.md ("se código e documentação divergirem, siga o código e reporte a divergência"), o Aggregate
Map — derivado diretamente do código — prevalece sobre `01-relational-model.md` nestes dois pontos. Não
alterei `01-relational-model.md` nesta Sprint: é um documento de uma tecnologia específica (SQL Server),
fora do princípio agnóstico desta Sprint, e sua correção pertence à Sprint que tratar da modelagem SQL
concreta. Registro aqui apenas para que a inconsistência não seja esquecida.

**Resolvido na Sprint 14.1:** ambos os pontos foram corrigidos em `01-relational-model.md` §0 (itens 1 e
2) — `WalletTags` agora referencia `UserId`, `Todos.ProjectId` agora é `NOT NULL`.

---

## 1. Modelo geral de fluxo

Para todo Aggregate desta Sprint, o fluxo conceitual é o mesmo:

```text
Aggregate (Domain)
    │  identidade própria, invariantes, ciclo de vida
    ▼
Persistence Boundary
    │  fronteira de consistência: o que deve ser lido/escrito
    │  atomicamente para o agregado permanecer válido
    ▼
Application
    │  orquestra casos de uso; decide quando tocar mais de um
    │  Aggregate na mesma operação; nunca implementa a leitura/escrita
    ▼
Infrastructure Adapter
    │  única camada que conhece a tecnologia de armazenamento real
```

A "Persistence Boundary" de cada agregado abaixo não é uma interface (proibido criar nesta Sprint) — é a
resposta conceitual a "o que precisa ser carregado e escrito junto para que este Aggregate nunca fique
inválido". É esse limite que vai virar a porta (`IUserRepository`, etc.) na Sprint em que portas forem
aprovadas.

---

## 2. Persistence Map por Aggregate

### 2.1 `User`

- **Identidade / ownership:** `UserId` é a identidade; o agregado é dono de `UserExperience` e de todo
  o histórico de `ExperienceEntry` (coleção filha, sem endereçamento fora do agregado).
- **1. Carrega sozinho?** Sim. Nenhuma invariante de `User` depende de ler outro Aggregate. A checagem
  de idempotência de XP (`(SourceType, SourceReferenceId, RewardType)`) é resolvida inteiramente dentro
  do próprio histórico de `Entries` — não precisa "ver" o `Habit`/`Todo`/`Project` de origem, apenas o
  identificador que já foi passado para dentro do agregado.
- **2. Atualiza sozinho?** Sim, para todas as suas próprias invariantes (perfil, sessão, XP). A exceção
  é unicidade de e-mail/nickname, que é uma restrição *entre instâncias* de `User` — não é resolvida
  carregando outro `User` na memória, é uma garantia que a camada de persistência precisa oferecer no
  momento da escrita (o Domain não tem, e não deveria ter, acesso a "todos os outros Users" para validar
  isso a cada escrita).
- **3. Consistência transacional entre Aggregates?**
  - **Não existe** para o próprio `User` isoladamente.
  - **Existe, com nuance, no fluxo de concessão de XP disparado por outro Aggregate** (Habit/RecurringTask/Todo/Project). A proteção real contra dupla concessão já vive inteiramente dentro de `User.Experience` (chave de deduplicação por origem). Isso significa que, se a escrita do `User` e a escrita do `Habit`/`Todo`/`Project` não puderem ser atômicas, o pior cenário é uma tentativa de reconciliação retornar a mesma entrada existente — nunca duplicar. **Conclusão: consistência eventual entre `User` e os agregados de atividade é aceitável para os fluxos com chave de origem estável** (RecurringTask/Todo/Project, cuja origem é o próprio `Id` da atividade).
  - **Exceção real, sem proteção de idempotência:** `Habit.RegisterPositive`/`RegisterNegative` gera XP com uma origem sempre nova (`Guid.NewGuid()`, ação repetível, não um "completion"). Sem chave estável, não há como uma reconciliação distinguir "essa concessão já aconteceu" de "essa é uma nova concessão legítima". Para este fluxo específico, a atualização de `Habit.PositiveCount`/`NegativeCount` e a concessão de XP em `User` **precisam** ser aplicadas dentro do mesmo limite transacional — é o único ponto de todo o Domain onde encontrei uma exigência real de consistência imediata entre dois Aggregates distintos.
- **4. Entidade filha?** `ExperienceEntry`. Ciclo de vida controlado inteiramente por `User`/`UserExperience` — é histórico append-only, nunca criada, lida ou removida fora do agregado.
- **5. Value Object persistido?** Nenhum. `EmailAddress`, `UserName`, `Nickname` são usados só como validadores no momento da chamada; o valor persistido é o primitivo já normalizado.
- **6. Lazy loading necessário?** Não como conceito de Domain — o Domain não modela referências preguiçosas (sem proxies, sem referência a outro Aggregate por objeto). O agregado deve ser tratado como carregado por completo sempre que uma invariante depende do estado completo (ex.: checar duplicidade de XP depende de enxergar o histórico relevante). Isso não impede que uma implementação futura resolva essa checagem específica como uma verificação de existência pontual em vez de materializar toda a lista em memória — é uma liberdade de implementação da Infrastructure, não uma exigência do Domain.
- **7. Consultas típicas (não implementar):** por `UserId`; por e-mail normalizado (login, checagem de duplicidade); por nickname normalizado (checagem de duplicidade); resumo para dashboard (projeção, não o agregado completo).
- **8. Operações típicas (não implementar):** criar; atualizar nome/e-mail; atualizar preferências; definir hash de senha; confirmar e-mail; registrar login; ativar/desativar; completar onboarding; completar perfil; invalidar sessões; conceder XP (grava em `Experience`/`Entries`).

### 2.2 `UserToken`

- **Identidade / ownership:** `TokenId` é a identidade; referencia `UserId` por identificador, não por containment.
- **1. Carrega sozinho?** Sim — por hash+tipo (para validar/consumir) ou por `(UserId, Tipo)` (para revogação em lote). Nunca precisa carregar `User` para validar seu próprio estado (tipo, expiração, uso, revogação são autocontidos).
- **2. Atualiza sozinho?** Sim, para marcar como usado ou revogado.
- **3. Consistência transacional entre Aggregates?** Aqui está a segunda (e última) exigência real de
  consistência imediata encontrada nesta Sprint: no fluxo de reset de senha, marcar o token como usado e
  alterar a senha/sessão do `User` são duas escritas em dois Aggregates diferentes. Se apenas a senha for
  persistida e a marcação do token falhar, o token de reset continua utilizável e pode ser reaproveitado
  — não é uma falha de autorização (quem não tem o token não pode explorá-la), mas é reprocessamento
  indevido de uma operação sensível de segurança. **Recomendação: este fluxo específico deve ser tratado
  com o mesmo limite transacional entre `UserToken` e `User`.** Fora deste fluxo (emissão de token,
  expiração natural, revogação em lote por tipo), consistência eventual é aceitável.
- **4. Entidade filha?** Nenhuma.
- **5. Value Object persistido?** Nenhum — `TokenHash` é uma string opaca já com hash aplicado antes de chegar ao Domain.
- **6. Lazy loading necessário?** Não — agregado pequeno, sempre carregado por completo.
- **7. Consultas típicas:** por hash+tipo (validação/consumo); lista ativa por `(UserId, Tipo)` (revogação em lote); por `Id`.
- **8. Operações típicas:** criar; marcar como usado; revogar; revogar em lote por tipo.

### 2.3 `Habit`

- **Identidade / ownership:** `HabitId`; referencia `UserId` por identificador.
- **1. Carrega sozinho?** Sim — por `(UserId, HabitId)` ou lista por `UserId`. Nenhuma invariante própria depende de outro Aggregate.
- **2. Atualiza sozinho?** Sim, para título/descrição/direção/dificuldade e para os próprios contadores.
- **3. Consistência transacional entre Aggregates?** Sim — ver §2.1, item 3 ("exceção real"): `RegisterPositive`/`RegisterNegative` + concessão de XP em `User` não têm proteção de idempotência do lado do XP (origem sempre nova), então essa dupla escrita precisa do mesmo limite transacional para não arriscar concessão perdida ou duplicada sob falha parcial.
- **4. Entidade filha?** Nenhuma.
- **5. Value Object persistido?** Nenhum.
- **6. Lazy loading necessário?** Não — agregado pequeno.
- **7. Consultas típicas:** por `(UserId, HabitId)`; lista por `UserId` ordenada por posição (quadro Daily).
- **8. Operações típicas:** criar; atualizar; registrar positivo; registrar negativo; excluir; reordenar (operação em lote sobre a lista de Habits do mesmo usuário — múltiplas instâncias do mesmo tipo de Aggregate, não um cruzamento de tipos).

### 2.4 `RecurringTask`

- **Identidade / ownership:** `TaskId`; referencia `UserId` por identificador.
- **1. Carrega sozinho?** Sim.
- **2. Atualiza sozinho?** Sim.
- **3. Consistência transacional entre Aggregates?** Diferente de `Habit`: a conclusão (`ToggleCompletion`) usa o próprio `Id` da tarefa como origem estável da concessão de XP — a mesma proteção por deduplicação de `User.Experience` descrita em §2.1 se aplica aqui. **Consistência eventual é aceitável** para este fluxo.
- **4. Entidade filha?** Nenhuma.
- **5. Value Object persistido?** Nenhum.
- **6. Lazy loading necessário?** Não.
- **7. Consultas típicas:** por `(UserId, TaskId)`; lista por `UserId` ordenada por posição.
- **8. Operações típicas:** criar; atualizar; alternar conclusão; excluir; reordenar (lote sobre Tasks do mesmo usuário).

### 2.5 `Project` (com `Todo` como entidade filha)

- **Identidade / ownership:** `ProjectId`; referencia `UserId` por identificador. É dono do ciclo de vida de `Todo`.
- **1. Carrega sozinho?** Sim, mas "sozinho" aqui inclui suas próprias `Todo` — elas são parte do mesmo agregado, não um agregado externo. Carregar `Project` sem seus `Todo` quebra a possibilidade de aplicar as invariantes de reordenação/containment.
- **2. Atualiza sozinho?** Sim, incluindo alterações nos seus `Todo` (adicionar, remover, mover posição, alternar conclusão) — tudo isso é escrita interna ao mesmo agregado.
- **3. Consistência transacional entre Aggregates?**
  - **Dentro do agregado (`Project` + `Todo`): consistência imediata é obrigatória** — é a própria definição de containment validada em 13.1 (exclusão de `Project` depende de `Todo` não existir fora dele).
  - **Entre `Project`/`Todo` e `User`:** mesma análise de §2.4 — a conclusão de um `Todo` ou de um `Project` usa o próprio `Id` como origem estável de XP, protegida por deduplicação em `User.Experience`. **Consistência eventual é aceitável** aqui.
  - **Caso especial — mover um `Todo` entre dois `Project`:** essa operação toca duas instâncias do mesmo tipo de Aggregate Root (`Project` de origem e `Project` de destino) na mesma operação de negócio. Isso não é uma violação — é uma operação legítima que precisa carregar e escrever duas instâncias do agregado na mesma unidade de trabalho da Application. Vale registrar explicitamente porque é o único cenário do Domain em que uma única operação de usuário mexe em duas instâncias do mesmo Aggregate Root ao mesmo tempo.
- **4. Entidade filha?** `Todo`. Ciclo de vida controlado inteiramente por `Project` — confirmado em 13.1 que não existe nenhuma forma de um `Todo` sobreviver à exclusão do seu `Project`.
- **5. Value Object persistido?** Nenhum — `Title`/`Description`/`Color` são validados via VO e persistidos como primitivos.
- **6. Lazy loading necessário?** Aqui está a exceção mais defensável do Domain para uma discussão sobre carregamento parcial: `Project.Status`/`Progress`/`NextTodo`/`LastUpdatedAtUtc` são todos computados a partir de `Todos`, e um projeto pode acumular uma lista grande de Todos ao longo do tempo. Ainda assim, **o Domain não expõe nenhuma operação que só precise do `Project` sem seus `Todo`** — toda invariante e toda propriedade computada depende da lista completa. Portanto, do ponto de vista do Domain, o agregado deve ser tratado como sempre carregado por completo; qualquer otimização de leitura parcial (ex.: um resumo de projeto sem a lista de Todos) é uma responsabilidade de um modelo de leitura/projeção separado — não do carregamento do próprio Aggregate para escrita.
- **7. Consultas típicas:** por `(UserId, ProjectId)` incluindo Todos; lista de Projects por `UserId` (possivelmente como resumo, sem a lista completa de Todos, para telas de listagem); por `(UserId, TodoId)` — precisa localizar o `Project` que contém aquele Todo.
- **8. Operações típicas:** criar/atualizar/arquivar/excluir Project; adicionar/atualizar/mover/excluir Todo; alternar conclusão de Todo (pode conceder XP); reordenar Projects (lote); reordenar Todos dentro do mesmo Project (lote, escopado).

### 2.6 `Wallet`

- **Identidade / ownership:** `WalletId`; referencia `UserId` por identificador; deliberadamente magro — sem entidades filhas, sem saldo armazenado.
- **1. Carrega sozinho?** Sim. A única invariante própria (um Wallet por User) não depende de carregar `Transaction`/`WalletTag`.
- **2. Atualiza sozinho?** Sim — os únicos campos mutáveis são metadados (timestamp de atividade).
- **3. Consistência transacional entre Aggregates?** Não é exigida para o `Wallet` em si. O saldo nunca é armazenado nele — é sempre calculado a partir de uma lista de `Transaction` fornecida de fora (`CalculateBalance` é um método stateless). Isso significa que `Wallet` nunca precisa estar transacionalmente amarrado a `Transaction` para permanecer válido; ele permanece válido sozinho o tempo todo.
- **4. Entidade filha?** Nenhuma.
- **5. Value Object persistido?** Nenhum.
- **6. Lazy loading necessário?** Não é uma questão aplicável — o agregado não tem nada para carregar de forma preguiçosa.
- **7. Consultas típicas:** por `UserId` (existência/obtenção do wallet do usuário atual); por `WalletId`.
- **8. Operações típicas:** criar (lazily, na primeira transação/tag); tocar timestamp de atividade.

### 2.7 `Transaction`

- **Identidade / ownership:** `TransactionId`; referencia `WalletId` e, opcionalmente, `WalletTagId`, ambos por identificador.
- **1. Carrega sozinho?** Sim, para leitura/edição/exclusão de uma transação específica. Não precisa carregar `Wallet` para suas próprias invariantes de valor/tipo/data.
- **2. Atualiza sozinho?** Sim, para os próprios campos.
- **3. Consistência transacional entre Aggregates?** Existe uma invariante de escrita que cruza `Wallet` e `WalletTag`: a tag referenciada precisa pertencer ao mesmo dono do Wallet. Essa é uma checagem de **validação no momento da escrita**, não uma exigência de consistência transacional forte contínua — uma vez que a transação é criada com uma referência válida, nada exige que `Wallet`/`WalletTag`/`Transaction` continuem sincronizados por uma transação de banco compartilhada depois disso. A validação em si precisa acontecer antes do commit da `Transaction` (ver risco de ownership cruzado em §3 do Aggregate Map de 13.1), mas isso é responsabilidade da Application (consultar os agregados envolvidos antes de decidir), não do `Transaction` carregar `Wallet`/`WalletTag` como parte do seu próprio agregado.
- **4. Entidade filha?** Nenhuma.
- **5. Value Object persistido?** Nenhum — `Description`/`Notes` validados e persistidos como primitivos.
- **6. Lazy loading necessário?** Não.
- **7. Consultas típicas:** por `Id`; lista por `WalletId` (extrato, cálculo de saldo/totais); lista por `WalletId` + intervalo de datas; lista por `WalletTagId` (para saber quantas transações usam uma tag antes de excluí-la).
- **8. Operações típicas:** criar; atualizar; excluir; atribuir/remover tag.

### 2.8 `WalletTag`

- **Identidade / ownership:** `TagId`; referencia `UserId` por identificador (não `WalletId` — ver §0).
- **1. Carrega sozinho?** Sim — por `(UserId, TagId)` ou lista por `UserId`.
- **2. Atualiza sozinho?** Sim, para nome/cor.
- **3. Consistência transacional entre Aggregates?** A exclusão de uma tag precisa refletir em toda `Transaction` que a referencia (remover a referência). Mesma natureza do item anterior: é uma operação de limpeza disparada por um evento de negócio (exclusão da tag), não uma exigência de que `WalletTag` e `Transaction` sejam sempre lidos/escritos como uma unidade. A Application, ao excluir uma tag, deve localizar as `Transaction` afetadas e atualizá-las — consistência eventual é aceitável aqui (uma janela curta em que uma transação ainda aponta para uma tag recém-excluída não quebra nenhuma invariante de leitura crítica, apenas precisa ser resolvida antes da tag ser reutilizada/recriada).
- **4. Entidade filha?** Nenhuma.
- **5. Value Object persistido?** Nenhum — `Name`/`Color` validados e persistidos como primitivos.
- **6. Lazy loading necessário?** Não.
- **7. Consultas típicas:** por `(UserId, TagId)`; lista por `UserId`; checagem de nome duplicado por `UserId`.
- **8. Operações típicas:** criar; renomear; mudar cor; excluir (com limpeza de referências em `Transaction`).

---

## 3. Ownership Matrix

| Aggregate | Identidade | Referencia (por identificador) | Contém (mesmo agregado) | Nunca referenciado por containment de |
|---|---|---|---|---|
| `User` | `UserId` | — | `UserExperience`, `ExperienceEntry[]` | `UserToken`, `Wallet`, `Habit`, `RecurringTask`, `Project`, `WalletTag` |
| `UserToken` | `TokenId` | `UserId` | — | `User` |
| `Habit` | `HabitId` | `UserId` | — | `User` |
| `RecurringTask` | `TaskId` | `UserId` | — | `User` |
| `Project` | `ProjectId` | `UserId` | `Todo[]` | `User` |
| `Todo` | `TodoId` | `UserId`, `ProjectId` (dono) | — | `User` (só referência) |
| `Wallet` | `WalletId` | `UserId` | — | `User` |
| `Transaction` | `TransactionId` | `WalletId`, `WalletTagId?` | — | `Wallet`, `WalletTag` |
| `WalletTag` | `TagId` | `UserId` | — | `Wallet`, `User` |

`Todo` não é um Aggregate Root independente — está listado por completude, mas seu ciclo de vida é de
`Project` (ver §2.5).

---

## 4. Transactional Boundaries

| Fronteira | Consistência exigida | Justificativa |
|---|---|---|
| Dentro de `User` (incl. `Experience`/`Entries`) | Imediata | Mesmo agregado — invariante de idempotência de XP precisa ver o histórico completo no momento da escrita. |
| Dentro de `Project` (incl. `Todos`) | Imediata | Mesmo agregado — ciclo de vida de `Todo` amarrado a `Project` (confirmado em 13.1). |
| `Habit` ↔ `User` (fluxo `RegisterPositive`/`RegisterNegative`) | **Imediata** | Única exceção real: origem de XP sem chave estável (`Guid.NewGuid()`), sem proteção de deduplicação — falha parcial pode perder ou duplicar XP sem chance de reconciliação segura. |
| `UserToken` ↔ `User` (fluxo de reset de senha) | **Imediata** (recomendada) | Falha parcial deixa um token de reset ainda válido reutilizável — risco de reprocessamento de operação sensível de segurança. |
| `RecurringTask`/`Todo`/`Project` ↔ `User` (conclusão → XP) | Eventual (aceitável) | Origem estável (`Id` da própria atividade) protegida por deduplicação dentro de `User.Experience` — reconciliação nunca duplica. |
| `Transaction` ↔ `Wallet`/`WalletTag` (ownership da tag) | Eventual (aceitável) | Validação de escrita pontual, não uma invariante que precisa permanecer sincronizada continuamente. |
| `WalletTag` ↔ `Transaction` (exclusão de tag) | Eventual (aceitável) | Limpeza de referência disparada por evento de negócio, sem janela de leitura crítica exposta. |
| Mover `Todo` entre dois `Project` | Imediata, entre duas instâncias do mesmo tipo de Aggregate | Ambas as instâncias precisam refletir o resultado da mesma operação de negócio — não é uma exceção ao modelo, é uma unidade de trabalho tocando dois agregados do mesmo tipo. |

Apenas **duas** fronteiras de todo o Domain exigem consistência imediata entre tipos de Aggregate
diferentes: `Habit ↔ User` (concessão de XP sem chave de deduplicação) e `UserToken ↔ User` (consumo de
token de segurança). Todas as demais interações cross-aggregate toleram consistência eventual porque a
invariante que protege a correção já vive dentro de um único agregado.

---

## 5. Loading Strategy

Nenhum Aggregate deste Domain define ou precisa de carregamento parcial/preguiçoso como parte do seu
contrato de comportamento — nenhuma entidade contém uma referência "preguiçosa" a outro Aggregate por
identidade de objeto, e toda invariante de escrita depende do estado completo do próprio agregado (nunca
de uma fração dele). `Project` é o caso mais próximo de justificar carregamento parcial (lista de Todos
potencialmente grande), mas mesmo ali toda operação de escrita e toda invariante depende da lista
completa — carregamento parcial, se algum dia existir, seria uma otimização de leitura (modelo de
projeção/consulta), nunca uma alteração ao que o Aggregate precisa para ser escrito com segurança.

Regra geral: **todo Aggregate é carregado por completo** antes de qualquer operação de escrita.

---

## 6. Update Strategy

Cada Aggregate é responsável por proteger suas próprias invariantes através de comportamento exposto
(nunca por atribuição direta de estado). A atualização de um Aggregate é sempre: carregar completo →
executar o comportamento de domínio correspondente → persistir completo. Nenhuma atualização parcial de
campos soltos é compatível com o Domain atual — todos os setters relevantes são privados e só acessíveis
através de métodos que também mantêm as invariantes (`Update`, `SetActive`, `ToggleCompletion`, etc.).

As duas exceções de §4 (consistência imediata cross-aggregate) implicam que a Application, ao orquestrar
esses dois fluxos específicos, precisa garantir que as duas escritas (`Habit`+`User`,
`UserToken`+`User`) sejam aplicadas como uma unidade — sem prescrever aqui *como* (é decisão de Sprint
futura, quando a tecnologia for escolhida).

---

## 7. Deletion Strategy

| Aggregate | O que acontece na exclusão |
|---|---|
| `User` | Fora do escopo desta Sprint — nenhuma operação de exclusão de conta existe hoje no Domain/Application. Registrar como lacuna para quando LGPD/exclusão de conta for endereçada. |
| `UserToken` | Exclusão física ou revogação (já suportado); não afeta outros Aggregates. |
| `Habit` / `RecurringTask` | Exclusão física direta; não afeta outros Aggregates (histórico de XP já concedido permanece em `User.Experience`, pois é uma cópia de dados no momento da concessão, não uma referência viva). |
| `Project` | Exclusão do `Project` **deve** eliminar todos os seus `Todo` — é o próprio containment (ver §2.5); não é uma decisão de "cascade" a ser configurada depois, é uma consequência estrutural do agregado. |
| `Todo` | Exclusão individual dentro do `Project` dono; nunca exclusão "solta" fora do agregado. |
| `Wallet` | Sem operação de exclusão hoje no Domain/Application (é criado lazily e nunca removido). Registrar como lacuna, mesma natureza da de `User`. |
| `Transaction` | Exclusão física direta; não afeta `Wallet` (saldo é sempre recomputado) nem `WalletTag` (a tag sobrevive à transação). |
| `WalletTag` | Exclusão exige limpar a referência em toda `Transaction` afetada (ver §4) — não exclui as transações, apenas remove a associação. |

---

## 8. Consultas típicas (consolidado, não implementar)

- Por identidade única do agregado (`Id` + `UserId` de ownership, quando aplicável).
- Lista por `UserId`, ordenada por posição, para os quadros de Habits/Tasks/Projects.
- Checagem de unicidade (e-mail, nickname, nome de tag) antes de criar/renomear.
- Lookup por chave alternativa: token por hash+tipo; usuário por e-mail normalizado.
- Consultas de leitura agregada (saldo, totais, dashboard) são projeções — não pertencem ao carregamento
  de um Aggregate para escrita, e não foram desenhadas nesta Sprint (proibido implementar consultas).

## 9. Operações típicas (consolidado, não implementar)

- Criar / Atualizar / Excluir para cada Aggregate Root.
- Operações de transição de estado específicas do domínio (`ToggleCompletion`, `RegisterPositive`,
  `ConfirmEmail`, `InvalidateSessions`, `MarkAsUsed`, `Revoke`, etc.) — nunca atribuição direta de campo.
- Operações em lote sobre a mesma lista de um mesmo usuário (reordenar Habits/Tasks/Projects/Todos).
- Concessão de XP como efeito colateral de uma operação de outro Aggregate (ver §4 para os limites de
  consistência exigidos em cada caso).
