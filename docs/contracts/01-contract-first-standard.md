# Padrão Contract-First

## 1. Definição adotada

No LevelUp, Contract-First significa que qualquer fronteira relevante é especificada antes da implementação concreta.

Fronteiras cobertas:

- UI → Application;
- HTTP → Application;
- Application → Persistence;
- Application → E-mail;
- Application → Cache;
- Application → Clock;
- Domain events → handlers;
- Health checks → operação;
- artefatos OpenAPI → consumidores futuros.

## 2. Fonte da verdade

| Assunto | Fonte da verdade |
|---|---|
| Entrada e saída pública | `LevelUp.Contracts` e OpenAPI |
| Regras de negócio | Domain |
| Orquestração | Application |
| Persistência | Infrastructure |
| Layout e interação | Web |
| Erros públicos | catálogo de erros |
| Compatibilidade | testes de contrato |

## 3. Processo obrigatório

Para cada nova funcionalidade:

1. descrever caso de uso;
2. definir request e response;
3. definir erros possíveis;
4. definir autorização e ownership;
5. definir idempotência;
6. criar testes de contrato;
7. criar command/query;
8. implementar domínio;
9. implementar adapter;
10. integrar UI;
11. atualizar OpenAPI e documentação.

## 4. Contratos não são entidades

Exemplo incorreto:

```csharp
public sealed record CreateHabitRequest(Habit Habit);
```

Exemplo correto:

```csharp
public sealed record CreateHabitRequest(
    string Title,
    string? Description,
    HabitDifficultyContract Difficulty,
    HabitDirectionContract Direction,
    ActivityAttributeContract? Attribute);
```

## 5. Contratos imutáveis

Usar preferencialmente `sealed record` com propriedades explícitas e sem comportamento de domínio.

## 6. Nullability

- campo obrigatório: não anulável;
- campo opcional: anulável;
- coleção ausente: coleção vazia, salvo diferença semântica explícita;
- não utilizar `default!` para esconder contrato incompleto.

## 7. Datas

- instantes: `DateTimeOffset` em UTC;
- data civil: `DateOnly`;
- duração: `TimeSpan` ou unidade explícita;
- contrato JSON: ISO 8601.

## 8. Dinheiro

- usar `decimal` internamente;
- contrato deve incluir moeda quando houver possibilidade futura de múltiplas moedas;
- nunca usar `double` para valores monetários.

## 9. IDs

IDs devem possuir semântica clara no código interno. No contrato HTTP, podem ser `Guid`, desde que documentados.

## 10. Idempotência

Operações sujeitas a repetição devem aceitar chave de idempotência ou possuir regra natural. Exemplos:

- conceder XP;
- processar evento externo;
- registrar transação originada por integração;
- confirmar e-mail;
- resetar senha.
