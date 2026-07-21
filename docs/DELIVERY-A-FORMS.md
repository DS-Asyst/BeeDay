# Delivery A — Forms

## Scope

This delivery introduces the reusable form layer of the LevelUp Design System.

## Components added

- `LevelUpInput`
- `LevelUpTextArea`
- `LevelUpSelect<TValue>`
- `LevelUpCheckbox`
- `LevelUpDateInput<TValue>`

## Editors migrated

- Habit editor
- Task editor
- To-Do editor
- Project editor

The editors preserve their models, validation annotations, commands, modal behavior, and current visual identity. Only repeated form markup was replaced.

## CSS

A new global stylesheet was added:

```text
src/LevelUp.Web/wwwroot/css/forms.css
```

It defines the default visual contract for fields, controls, textareas, counters, disabled/read-only states, focus states, and checkboxes.

## Validation checklist

Run from the solution root:

```bash
dotnet clean
dotnet restore
dotnet build
dotnet test
dotnet run --project src/LevelUp.Web/LevelUp.Web.csproj
```

Then validate manually:

1. Create and edit a Habit.
2. Create and edit a Task.
3. Create and edit a To-Do with and without a due date.
4. Create and edit a Project.
5. Submit each editor without a title and confirm that validation is displayed.
6. Confirm that the Notes counter updates correctly.
7. Confirm that select values persist after saving and reopening an editor.

## Correção de compatibilidade Razor

Os rótulos obrigatórios dos componentes de formulário usam um bloco Razor explícito para renderizar o asterisco. Essa estrutura evita o erro `RZ1008`, que ocorre quando markup é colocado dentro de uma instrução de controle de fluxo em linha.

Também foram normalizadas as instruções `if` dos editores e removidas diretivas `using` redundantes dos code-behind de Dashboard e Profile.
