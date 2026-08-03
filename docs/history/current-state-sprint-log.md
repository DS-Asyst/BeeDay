# Estado Atual

> **Atualização Sprint 14.6:** os quatro bloqueadores do §3.6 e a lista de "características confirmadas"
> do §2 abaixo descrevem o estado **antes** do corte para SQL Server. Esse corte foi concluído na Sprint
> 14.6: SQL Server é o único provider de runtime; `ILevelUpRepository`/`GetLevelUpResponse` foram
> removidos (zero consumidores); todo handler de escrita usa um dos 8 contratos por Aggregate ou
> `IUnitOfWork`. JSON existia apenas como código legado, não registrado em DI, sem leitura ou escrita em
> runtime.
>
> **Atualização Sprint 14.7:** esse código legado foi **removido do repositório**, não apenas
> desregistrado — `LevelUpData` (Domain), toda a pasta `Infrastructure/Persistence/Json/`,
> `JsonStorageOptions` e `JsonStorageHealthCheck` não existem mais. Nenhum tipo mencionado nas seções
> abaixo (`ILevelUpRepository`, `LevelUpData`, `JsonStorageGate`, `JsonDashboardReadService`,
> `JsonWalletReadService`, `DomainJsonContractResolver`, `JsonSerializerOptionsFactory`) existe no
> código atual — todas essas referências são puramente históricas. `JsonEventJournal` é o único
> componente com "Json" no nome que permanece, e é auditoria de domain events (write-only), não
> persistência funcional. Ver `docs/architecture/08-migration-status.md` §9 para o estado atual
> completo, verificado contra o código. O restante deste documento permanece como registro histórico
> do problema original — nenhuma seção abaixo descreve o estado presente do código.

## 1. Solução

A solução atual possui quatro projetos de produção:

```text
LevelUp.Domain
LevelUp.Application
LevelUp.Infrastructure
LevelUp.Web
```

E quatro projetos de testes correspondentes.

## 2. Características confirmadas no código

- Blazor Server com componentes interativos no servidor.
- Autenticação por cookie.
- MediatR para commands, queries, handlers e domain-event notifications.
- FluentValidation na pipeline de aplicação.
- Entidades e value objects no domínio.
- Persistência JSON com escrita temporária, backups e recuperação.
- `LevelUpData` como raiz global de persistência.
- Desde a Sprint 12.8, `LevelUp.Domain` não referencia `System.Text.Json` em nenhum tipo — todo
  contrato de serialização (setters privados, propriedades computadas ignoradas, os três renames
  históricos) é reconstruído a partir da Infrastructure por um `IJsonTypeInfoResolver` dedicado
  (`DomainJsonContractResolver`), configurado exclusivamente dentro de
  `JsonSerializerOptionsFactory`. Ver `docs/data/03-json-to-sql-transition.md` §6.
- `ILevelUpRepository` expondo `LoadAsync`, `SaveAsync` e `UpdateAsync` sobre o documento completo —
  ainda o contrato usado por todo handler de escrita (ver §3.6).
- Desde a Sprint 13.4, dois fluxos de leitura (Dashboard, Wallet) não passam mais por
  `ILevelUpRepository`: usam `IDashboardReadService`/`IWalletReadService`, cada um com adapter JSON
  próprio (`JsonDashboardReadService`/`JsonWalletReadService`) sobre o mesmo `JsonLevelUpDocumentStore`
  interno. Oito portas de escrita por Aggregate (Sprint 13.3) existem no código mas não têm adapter
  nem consumidor — ver `docs/architecture/08-migration-status.md`.
- Isolamento por `UserId` em diversos handlers e snapshots.
- `CurrentUserId` ainda presente no documento persistido (bootstrapping/migração legada), mas
  não é mais utilizado como fallback de autenticação desde a Sprint 12.5 — `ICurrentUserContext`
  é obrigatório em todos os handlers.
- Wallet, atividades, projetos, identidade, tokens e experiência no mesmo grafo persistido.
- Pipeline de CI com restore, format, build, tests e publish.
- Deploy para IIS com health checks e rollback.

## 3. Problemas arquiteturais que motivam a mudança

### 3.1 Contrato acoplado ao armazenamento

O contrato atual de repositório retorna `LevelUpData`. Isso faz com que a camada de aplicação conheça a forma física usada pelo JSON. Um banco relacional não trabalha naturalmente com um único documento global.

### 3.2 Unidade de trabalho global

Qualquer alteração é feita dentro de uma mutação do documento completo. Isso cria serialização global, alto acoplamento e limita concorrência.

### 3.3 Modelos compartilhados em excesso

Entidades de domínio, modelos de persistência, respostas de aplicação e estado da UI estão próximos demais em alguns fluxos. Contract-First exige fronteiras explícitas.

### 3.4 Usuário atual persistido

`CurrentUserId` pertence ao contexto da requisição ou circuito autenticado, não ao banco. Desde a
Sprint 12.5, nenhum handler de Application usa mais esse campo para resolver o usuário
autenticado (`ICurrentUserContext` é obrigatório); o campo persiste no JSON apenas como
bootstrapping/migração legada, a ser removido quando a persistência relacional eliminar a
necessidade de um documento único.

### 3.5 Mudança de banco com risco de efeito cascata

Sem contratos específicos, a substituição por EF Core pode exigir alterações simultâneas em handlers, testes e componentes.

### 3.6 Bloqueadores confirmados para a substituição relacional (Sprint 12.8; status atualizado na Sprint 13.7)

A Sprint 12.8 removeu o acoplamento de `LevelUp.Domain` ao formato JSON (§2 acima) e confirmou quatro
bloqueadores em Application/Infrastructure para trocar apenas a Infrastructure por um provider SQL
Server. A EPIC 13 (Sprints 13.1–13.6) tratou os quatro — **dois estão parcialmente resolvidos, dois
seguem exatamente como estavam**. Status verificado contra o código atual; ver
`docs/architecture/08-migration-status.md` para o inventário completo, arquivo por arquivo:

- **`ILevelUpRepository` ainda expõe o documento `LevelUpData` inteiro** — **parcialmente resolvido**.
  Oito portas por Aggregate foram definidas (Sprint 13.3, `docs/architecture/07-persistence-contracts.md`
  §3.1), mas nenhuma tem adapter, registro em DI ou consumidor — `ILevelUpRepository` continua sendo o
  único contrato de escrita efetivamente usado por todo handler de comando (Habits, Tasks, Todos,
  Projects, Ordering, Wallet, Users, Authentication, Identity).
- **`GetLevelUpResponse` ainda expõe `LevelUpData` diretamente como resposta** — **parcialmente
  resolvido**. `IDashboardReadService`/`DashboardResponse` substituíram esse caminho para a tela
  `/daily` (Sprint 13.4). `GetLevelUpResponse` continua existindo e sendo retornado para 3 consumidores
  não migrados (`Tutorial.razor`, `Account.razor`, `ProfileCreationState`) — o tipo não foi removido
  porque ainda tem consumidores reais.
- **Vários handlers de Application operam diretamente sobre o agregado global** — **parcialmente
  resolvido, apenas para leitura**. Os 4 handlers de consulta de Wallet e o handler de consulta do
  Dashboard foram reescritos para read services dedicados (`IWalletReadService`, `IDashboardReadService`,
  ambos com adapter JSON real e testados). **Nenhum handler de escrita foi migrado** — todos continuam
  usando `ILevelUpRepository.UpdateAsync(Action<LevelUpData>)` inalterado.
- **`JsonStorageGate` é uma estratégia de concorrência específica de um único arquivo** — **inalterado,
  como esperado**. Reconfirmado pela auditoria de isolamento de persistência da Sprint 13.5: continua
  sendo uma decisão correta e exclusiva do adapter JSON, sem influenciar o desenho de concorrência do
  SQL Server (`rowversion`/transações reais).

Até que os quatro itens acima estejam totalmente resolvidos, a afirmação "trocar JSON por SQL Server
mudando apenas a Infrastructure" **ainda não é verdadeira** — apenas os dois fluxos de leitura migrados
(Dashboard, Wallet) já satisfazem essa propriedade hoje.

## 4. Capacidades que devem ser preservadas

- regras de XP e level-up;
- ownership por usuário;
- Wallet separada de XP;
- confirmação de e-mail e reset de senha;
- domain events;
- validações atuais;
- CI e quality gates;
- design system;
- health checks;
- configuração segura de produção;
- comportamento funcional atual aprovado.
