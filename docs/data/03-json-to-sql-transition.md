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

## 6. Boundary do JSON — implementado (Sprint 12.8)

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

### O que isso NÃO resolve (bloqueadores confirmados, não redesenhados nesta Sprint)

`ILevelUpRepository` ainda expõe `LevelUpData` inteiro, `GetLevelUpResponse` ainda expõe
`LevelUpData` diretamente, vários handlers de Application ainda operam sobre o agregado global, e
`JsonStorageGate` continua sendo uma estratégia de concorrência específica de arquivo único. Trocar
JSON por SQL Server **não** é hoje uma mudança isolada à Infrastructure — esses quatro pontos
exigem o redesenho do Contract-First (Sprint 13+). Ver
`docs/architecture/01-current-state.md` §3.6 para o detalhamento completo.
