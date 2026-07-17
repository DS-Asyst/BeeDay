# LevelUp

Aplicação console em .NET para organizar hábitos, projetos, capítulos, quests, chefes, leitura e carteira como uma jornada de progressão pessoal.

## Estado atual

A consolidação 8.5 centraliza recompensas e progressão antes da adoção de SQLite/EF Core.

- Hábito concluído: **0,5 XP**.
- Quest concluída: **1 XP**.
- Capítulo concluído: bônus equivalente à soma do XP de suas quests.
- Toda atividade produz um `Reward`.
- Somente `Character.ApplyReward()` altera XP, atributos e títulos.
- Projetos possuem atributo principal; quests vinculadas o herdam.
- Quests independentes podem escolher atributo próprio.
- Carteira usa movimentação assinada: valor, descrição, tag e data automática.
- Ativações e transições são orquestradas pelos serviços/workflows.

## Executar

```bash
dotnet restore
dotnet build LevelUp.slnx
dotnet test LevelUp.slnx
cd LevelUp
dotnet run
```

O arquivo `LevelUp/Data/save.json` contém um Roadmap ASP.NET Core limpo para testar a progressão completa.

## Documentação mantida

- `LevelUp/docs/Architecture.md`
- `LevelUp/docs/DecisionLog.md`
- `LevelUp/docs/TestingStrategy.md`
- `LevelUp/docs/Phase9Plan.md`


## Navegação atual

- **Personagem:** Perfil e Conquistas.
- **Diário:** Treinamentos, Missões e Projetos.
- **Inventário:** Biblioteca e Carteira.

Na Biblioteca, o progresso solicita somente a página atual. A conclusão do livro concede XP uma única vez: 1 XP abaixo de 100 páginas ou 10% do total de páginas, arredondado para baixo, a partir de 100 páginas.
