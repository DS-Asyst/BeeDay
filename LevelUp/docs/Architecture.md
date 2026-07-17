# Arquitetura

## Visão geral

O LevelUp adota separação entre núcleo, infraestrutura e cliente executável.

```text
LevelUp.Console
  ├── LevelUp
  └── LevelUp.Infrastructure
          └── LevelUp

LevelUp.Tests
  ├── LevelUp
  └── LevelUp.Infrastructure
```

A dependência sempre aponta para dentro: a infraestrutura conhece as interfaces do núcleo; o núcleo não conhece EF Core, SQLite ou detalhes do sistema operacional.

## Camada central — `LevelUp`

Responsabilidades:

- entidades e regras de domínio;
- `Reward` e progressão;
- workflows transacionais em nível de aplicação;
- serviços de hábitos, projetos, missões, capítulos, livros e carteira;
- abstração `IGameDataStore`;
- interface de console reutilizável.

Restrições:

- nenhuma referência a `Microsoft.EntityFrameworkCore`;
- nenhuma SQL;
- nenhuma dependência de caminho físico de banco;
- regras de negócio não podem ser implementadas no projeto Infrastructure.

## Infraestrutura — `LevelUp.Infrastructure`

Responsabilidades:

- conexão SQLite;
- `LevelUpDbContext`;
- migrations;
- repositórios de agregados;
- transações;
- aplicação automática de migrations;
- persistência exclusiva no banco SQLite.

A infraestrutura implementa `IGameDataStore`. O armazenamento relacional é intercambiável sem alterar workflows ou telas.

## Cliente — `LevelUp.Console`

É o composition root. Ele:

1. configura codificação do terminal;
2. cria o `SqliteGameDataStore`;
3. injeta o armazenamento no `ApplicationBootstrap`;
4. inicia o menu.

Nenhuma regra de negócio deve ser adicionada ao cliente.

## Estratégia de dados

A Fase 9 usa tabelas por agregado com payload serializado. Essa é uma etapa intermediária consciente:

- fornece transações, migrations, índices e inspeção por SQLite;
- preserva compatibilidade com o domínio atual;
- evita anemizar entidades apenas para atender ao ORM;
- permite normalização seletiva futura quando consultas Blazor exigirem projeções SQL.

## Fluxo de gravação

```text
Tela → Serviço/Workflow → GameStateService → IGameDataStore → transação SQLite
```

O snapshot inteiro é confirmado atomicamente. `Character.ApplyReward()` permanece como único ponto autorizado a alterar XP, atributos e títulos.

## Preparação para Blazor

Uma interface Blazor futura deverá referenciar `LevelUp` e usar casos de uso da aplicação. Ela não deverá acessar o `DbContext` diretamente. Consultas específicas poderão ser introduzidas por interfaces de leitura na camada central e implementadas na infraestrutura.
