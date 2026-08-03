# Transição JSON → SQL sem Migração de Dados

## 1. Decisão

Nenhum dado existente em JSON será levado ao novo banco.

Isso inclui:

- usuários;
- hashes de senha;
- tokens;
- hábitos;
- tarefas;
- projetos;
- To-Dos;
- XP;
- Wallet;
- transações;
- tags;
- auditoria;
- backups.

## 2. Consequência operacional

Ao ativar SQL Server:

- todas as contas deverão ser criadas novamente;
- todos os usuários começarão sem atividades;
- XP e Wallet começarão zerados;
- tokens antigos deixarão de funcionar;
- cookies antigos devem ser invalidados;
- o JSON permanece somente como backup histórico fora da aplicação durante o período de retenção definido.

## 3. Estratégia de transição

### Passo A — contratos

Implementar contratos e portas sem alterar o provider ativo.

### Passo B — adapter JSON

Adaptar o JSON atual às novas portas. O objetivo é comprovar que handlers e UI não dependem mais de `LevelUpData`.

### Passo C — adapter EF

Implementar SQL Server com os mesmos contratos.

### Passo D — testes de conformidade

Executar a mesma suíte contra JSON e SQL.

### Passo E — ambiente HMG vazio

Subir HMG com provider SQL Server e base nova.

### Passo F — validação funcional

Executar E2E e validação manual.

### Passo G — corte de produção

1. colocar versão anterior em manutenção;
2. fazer backup final do JSON por segurança histórica;
3. publicar versão SQL;
4. aplicar migration inicial;
5. remover cookies antigos por mudança de nome ou Data Protection purpose;
6. verificar readiness;
7. validar criação da primeira conta;
8. liberar acesso.

## 4. O que não fazer

- não realizar dual-write;
- não ler JSON como fallback após ativar SQL;
- não misturar IDs legados e novos;
- não criar migration improvisada posteriormente;
- não deixar o usuário acreditar que os dados antigos reaparecerão.

## 5. Compatibilidade preservada

“Preservar compatibilidade” significa preservar o comportamento funcional e os contratos aprovados durante a troca do adapter. Não significa preservar dados antigos.

## 6. Boundary do JSON — implementado (Sprint 12.8), código removido (Sprint 14.7)

> **Todo o código descrito nesta seção 6 foi removido na Sprint 14.7** — `DomainJsonContractResolver`,
> `JsonSerializerOptionsFactory`, os três migradores legados, e `SchemaCompatibilityCharacterizationTests`/
> `DomainJsonContractResolverTests` não existem mais no repositório. `LevelUp.Domain` continua sem
> referenciar `System.Text.Json` (fato preservado, ver `docs/architecture/01-current-state.md`
> §DomainAssemblyBoundaryTests), mas não porque um resolver externo reconstrói o contrato — não há mais
> nenhum contrato de serialização para `LevelUpData` reconstruir, porque `LevelUpData` também foi
> removido (§7 abaixo). Esta seção permanece como registro histórico de como o boundary funcionava
> entre a Sprint 12.8 e a Sprint 14.6.

`LevelUp.Domain` não referencia mais `System.Text.Json` (nenhum `[JsonInclude]`, `[JsonPropertyName]`
ou `[JsonIgnore]` em nenhuma entidade). Todo o contrato de serialização é reconstruído a partir da
Infrastructure por `DomainJsonContractResolver`
(`src/LevelUp.Infrastructure/Persistence/Json/DomainJsonContractResolver.cs`), um
`IJsonTypeInfoResolver` registrado apenas dentro de `JsonSerializerOptionsFactory`:

- toda propriedade com um campo de apoio de auto-propriedade (`<Nome>k__BackingField`) tem seu
  setter — privado, protegido ou público — religado via reflexão, substituindo `[JsonInclude]`;
- toda propriedade pública sem campo de apoio (computada, incluindo o override
  `Project.Completed`, que tem um setter vazio mas nenhum campo real) é removida do contrato,
  substituindo `[JsonIgnore]`;
- três renomeações históricas (`UserExperience.Entries` → `"Transactions"`,
  `LevelUpData.LegacyProfile` → `"profile"`, `LevelUpData.LegacyTodos` → `"todos"`) são aplicadas
  explicitamente por nome, substituindo `[JsonPropertyName]`;
- o resolver só atua sobre tipos do assembly `LevelUp.Domain` — outros tipos serializados pelas
  mesmas opções (ex.: o envelope anônimo de `JsonEventJournal`) continuam com o contrato padrão.

O schema JSON persistido não mudou: mesmos nomes de propriedade, mesma forma de documento, mesmos
três migradores legados (`LegacyActivityAttributeMigrator`, `LegacyCharacterMigrator`,
`LegacyInventoryTagMigrator`), mesmo comportamento de backup/restore/atomic write. Validado por
`SchemaCompatibilityCharacterizationTests` (fixture do schema pré-Sprint-2/3, inalterada) e por
`DomainJsonContractResolverTests` (ausência de propriedades computadas no JSON, nomes renomeados
preservados, setters privados populados através de coleções aninhadas).

### Migradores legados — classificação

Todos os três são **temporários — removíveis quando o adapter JSON for desativado**: operam
apenas sobre o formato de arquivo em disco (via `JsonNode`, antes da desserialização), e a decisão
da §1 acima ("nenhum dado JSON será migrado para SQL Server") significa que, no dia em que o
provider SQL se tornar o único ativo, não haverá mais nenhum arquivo JSON histórico para ler —
tornando os três migradores obsoletos por definição, não por reescrita.

- `LegacyActivityAttributeMigrator` — remove valores "Wisdom"/"Charisma" de `attribute`.
- `LegacyCharacterMigrator` — funde o extinto Character em User (schema ≤ 5).
- `LegacyInventoryTagMigrator` — renomeia Inventory → Wallet (schema ≤ 6).

Nenhum é "obrigatório" ou "documentação histórica apenas" — todos os três ainda são exercitados
por instâncias JSON reais em uso (Development/HMG) enquanto o adapter JSON permanecer ativo.

### Sprint 14.6 — corte de runtime concluído (Passos A–D desta seção)

`ILevelUpRepository`/`GetLevelUpResponse` foram removidos (zero consumidores); todo handler de escrita
usa um dos 8 contratos por Aggregate ou `IUnitOfWork`; SQL Server é o único provider ativo em runtime.
`JsonStorageGate` e o resto do pipeline JSON (`JsonLevelUpDocumentStore` e abaixo) permanecem no
repositório como código legado, **não registrados em DI** — nenhuma leitura ou escrita acontece por
esse caminho enquanto a aplicação roda. `JsonStoragePaths`/`JsonSerializerOptionsFactory` são a única
exceção, mantidos apenas porque `JsonEventJournal` (auditoria de domain events, um mecanismo à parte,
não relacionado à persistência de `LevelUpData`) ainda depende deles. Os três migradores legados (§
"Migradores legados" acima) seguem existindo no código, mas não são mais exercitados por nenhuma
instância em execução — o próprio adapter que os chamava (`JsonLevelUpRepository`) foi removido.

Isso conclui os Passos A–D desta seção (contratos, adapter JSON como referência, adapter EF, suíte
migrada) no sentido de código/runtime local. Os Passos E–G (ambiente HMG vazio, validação manual,
corte de produção real) são ações operacionais de deploy, fora do escopo desta Sprint — ver
`docs/architecture/08-migration-status.md` §8 para o estado de código verificado e
`docs/operations/01-operations.md` para o runbook de deploy quando esses passos forem executados.

### Sprint 14.7 — remoção completa do código JSON (não apenas do DI)

A Sprint 14.6 desregistrou o pipeline JSON (parágrafo acima); a Sprint 14.7 **remove o código em si** —
a decisão que a 14.6 deliberadamente deixou em aberto. Removidos: a pasta inteira
`src/LevelUp.Infrastructure/Persistence/Json/` (14 arquivos, incluindo `JsonLevelUpDocumentStore`,
`JsonStorageGate` e os três migradores legados), `JsonStorageOptions`, `JsonStorageHealthCheck`, e
`LevelUpData`/`LevelUpData.Persistence.cs` (Domain — ver `docs/architecture/08-migration-status.md`
§9.2 para o inventário completo de para onde cada invariante que `LevelUpData` impunha migrou).
`JsonStoragePaths`/`JsonSerializerOptionsFactory` também saem — `JsonEventJournal` (auditoria de
domain events, não persistência funcional) foi desacoplado deles, ganhando sua própria configuração
mínima (`EventJournalOptions`) e resolução de caminho inline. Não há mais nenhum código de persistência
JSON no repositório; `JsonEventJournal` é o único componente com "Json" no nome que permanece, e não
participa da persistência de nenhum Aggregate. Nenhuma migration foi criada ou alterada — `LevelUpData`
nunca teve mapeamento EF Core, então sua remoção não tem impacto de schema
(`dotnet ef migrations has-pending-model-changes` confirma). Ver
`docs/architecture/08-migration-status.md` §9 para o relato completo, arquivo por arquivo.
