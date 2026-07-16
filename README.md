# LevelUp

> Transforme evolução pessoal real em progressão de RPG.

## Visão geral

LevelUp é uma aplicação de produtividade gamificada construída com C# e .NET. O código permanece em inglês; toda a experiência do jogador é apresentada em português brasileiro.

## Funcionalidades atuais

- personagem, níveis, experiência e atributos;
- treinamentos recorrentes;
- projetos, capítulos, missões e chefes;
- Diário como central da jornada;
- Biblioteca com até dois livros simultaneamente em andamento;
- histórico de leitura, progresso por páginas e XP de leitura;
- Mochila como hub de recursos pessoais;
- Carteira para controle de dinheiro real;
- depósitos, retiradas justificadas, saldo, histórico e resumo mensal;
- persistência JSON centralizada;
- testes automatizados e CI.

## Navegação principal

```text
Personagem
Diário
├── Treinamentos
├── Missões
├── Projetos
└── Capítulos
Biblioteca
Mochila
└── Carteira
```

## Arquitetura

```text
Presentation (Spectre.Console)
        ↓
Application Workflows
        ↓
Domain Services
        ↓
Domain
        ↓
Persistence (JSON)
```

## Qualidade

```bash
dotnet restore LevelUp.slnx
dotnet format LevelUp.slnx --verify-no-changes
dotnet build LevelUp.slnx
dotnet test LevelUp.Tests/LevelUp.Tests.csproj
dotnet run --project LevelUp/LevelUp.csproj
```

## Princípios de produto

- dinheiro da Carteira representa patrimônio real, não moeda fictícia;
- gastos não afetam vida ou energia nesta fase;
- Biblioteca é independente de Projetos e Missões;
- no máximo dois livros podem permanecer em andamento;
- cada avanço de leitura fica registrado por data;
- todo texto visível ao jogador permanece em português.
