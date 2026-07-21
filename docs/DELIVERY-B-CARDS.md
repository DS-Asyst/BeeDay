# Entrega B — Cards

## Objetivo

Reduzir a duplicação estrutural dos cards do dashboard sem alterar regras de negócio, comportamento ou identidade visual.

## Componentes adicionados

### `LevelUpCard`

Shell semântico reutilizável baseado em `article`. Recebe conteúdo, classes contextuais e atributos HTML adicionais.

### `LevelUpCardMenu`

Componente responsável por:

- botão de opções;
- estado aberto/fechado;
- camada de fechamento externo;
- ações Edit e Delete;
- atributos de acessibilidade;
- fechamento automático antes de disparar a ação.

## Componentes migrados

- `HabitCard`
- `ActivityPreviewCard`, utilizado por Tasks, To-Dos e Projects

## Preservado

- regras de cores dos Habits;
- pontuação positiva e negativa;
- conclusão e reabertura das atividades;
- destaque de itens favoritos;
- metadados específicos;
- eventos de edição e exclusão;
- layout responsivo e estados de hover.

## CSS

Os estilos anteriormente isolados em cada card foram consolidados em:

```text
wwwroot/css/cards.css
```

O menu compartilhado continua utilizando o contrato global `card-action-menu`.

## Validação local

```bash
dotnet clean
dotnet restore
dotnet build
dotnet test
dotnet run --project src/LevelUp.Web/LevelUp.Web.csproj
```
