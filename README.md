# LevelUp

LevelUp é uma plataforma de evolução pessoal gamificada desenvolvida em C# e .NET 10. A aplicação usa uma interface de terminal com Spectre.Console, mantém todo o código em inglês e apresenta a experiência do usuário em português.

## Estado atual

- personagem com classe, nível, experiência, atributos e título de progressão;
- diário com treinamentos, missões, projetos e capítulos;
- projetos com um chefe final obrigatório;
- conquistas profissionais desbloqueadas ao derrotar o chefe final;
- biblioteca com histórico de leitura e experiência por páginas;
- mochila com carteira financeira real;
- persistência JSON, workflows, componentes reutilizáveis e testes automatizados.

## Navegação principal

- Personagem
- Diário
- Biblioteca
- Mochila
- Configurações
- Salvar jogo
- Sair

## Executar

```bash
dotnet restore LevelUp.slnx
dotnet build LevelUp.slnx
dotnet test LevelUp.Tests/LevelUp.Tests.csproj
dotnet run --project LevelUp/LevelUp.csproj
```

## Documentação

A documentação oficial está em `LevelUp/docs`:

- `Vision.md`
- `Architecture.md`
- `Domain.md`
- `GameTerminology.md`
- `Roadmap.md`
- `Phase5.md`
- `DecisionLog.md`
- `CHANGELOG.md`
- `Contributing.md`
