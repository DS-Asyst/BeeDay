# LevelUp — Game Terminology

Este documento define o vocabulário oficial do LevelUp.

O projeto utiliza nomes reais e claros na arquitetura e nas regras de negócio, enquanto a interface apresenta esses conceitos com uma linguagem inspirada em RPG.

## Princípio

A camada de domínio deve representar o significado real da funcionalidade.

A camada de apresentação pode utilizar metáforas de RPG, desde que não altere o significado do domínio.

## Terminologia oficial

| Conceito real | Código e domínio | Apresentação na UI |
|---|---|---|
| Usuário e sua progressão | `Character` | Character |
| Atividade recorrente | `Habit` | Training |
| Tarefa com conclusão única | `Quest` | Quest |
| Conjunto organizado de tarefas | `Project` | Project |
| Marco importante de um projeto | `Milestone` | Boss |
| Experiência global | `Experience` | XP |
| Experiência de atributo | `AttributeExperience` | Attribute XP |
| Característica evolutiva | `Attribute` | Attribute |
| Moeda e recompensa | `Gold` | Gold |
| Recompensa por uma ação | `Reward` | Reward |
| Conquista desbloqueável | `Achievement` | Achievement |
| Título concedido ao personagem | `Title` | Title |

## Regras de nomenclatura

### Habit e Training

No código, uma atividade recorrente deve ser chamada de `Habit`.

Na interface, ela deve ser apresentada como `Training`.

Exemplos:

```text
Habit
HabitService
CreateHabit()
CompleteHabit()