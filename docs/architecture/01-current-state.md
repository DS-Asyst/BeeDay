# Estado Atual

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
- `ILevelUpRepository` expondo `LoadAsync`, `SaveAsync` e `UpdateAsync` sobre o documento completo.
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

### 3.6 Bloqueadores confirmados para a substituição relacional (Sprint 12.8)

A Sprint 12.8 removeu o acoplamento de `LevelUp.Domain` ao formato JSON (§2 acima), mas
confirmou — sem redesenhar nesta Sprint — que os itens abaixo em Application/Infrastructure
ainda impedem trocar apenas a Infrastructure por um provider SQL Server. Nenhum destes é uma
regressão desta Sprint; são a continuação de 3.1/3.2, agora explicitamente marcados como
bloqueadores do Contract-First:

- **`ILevelUpRepository` ainda expõe o documento `LevelUpData` inteiro** (não portas por
  agregado) — qualquer novo adapter (SQL) precisaria ou reconstruir o documento inteiro em
  memória a cada chamada, ou esperar o redesenho de `docs/architecture/02-target-architecture.md`
  §3.
- **`GetLevelUpResponse` (Application/Features/Dashboard/Responses) ainda expõe `LevelUpData`
  diretamente como resposta** — o mesmo redesenho de contratos de saída (§3.3) é pré-requisito
  antes de trocar o provider sem tocar Application/Web.
- **Vários handlers de Application (Identity, Wallet, Habits, Tasks, Projects, Todos) operam
  diretamente sobre o agregado global** via `ILevelUpRepository.UpdateAsync(Action<LevelUpData>)`
  — cada um precisaria ser reescrito para uma porta por agregado no Contract-First.
- **`JsonStorageGate` é uma estratégia de concorrência específica de um único arquivo** (semáforo
  de processo inteiro) — não deve motivar o desenho de concorrência do SQL Server (que usa
  `rowversion`/transações reais, ver `docs/data/02-ef-core-strategy.md` §5); é uma decisão
  correta para JSON e exclusiva dela.

Nenhuma correção de código foi feita para estes quatro itens nesta Sprint — são, por decisão
explícita, trabalho do Contract-First (Sprint 13+), não desta Sprint. Até lá, a afirmação "trocar
JSON por SQL Server mudando apenas a Infrastructure" **não é verdadeira**: os quatro pontos acima
também precisarão mudar.

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
