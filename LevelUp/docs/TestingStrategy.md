# Testing Strategy

Os testes são organizados por funcionalidade, não por fases históricas.

## Prioridades

1. Recompensas: XP de hábito, quest e bônus de capítulo; aplicação única em `Character.ApplyReward()`.
2. Progressão: ativação sequencial de projetos, capítulos, quests e boss.
3. Regressão: quest concluída sempre concede 1 XP antes de persistir.
4. Atributos: herança do projeto e bloqueio de seleção manual em quests vinculadas.
5. Persistência: round-trip, validação, recuperação de corrupção e compatibilidade de schema.
6. Carteira: valores assinados, data automática, tags ordenadas e saldo.

## Comandos

```bash
dotnet test LevelUp.slnx
dotnet test LevelUp.slnx --collect:"XPlat Code Coverage" --settings coverlet.runsettings
```
