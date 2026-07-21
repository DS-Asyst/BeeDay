# Arquitetura do frontend LevelUp

## Princípio

`LevelUp.Web` é a camada de apresentação. Regras de negócio permanecem em Domain e Application; persistência permanece em Infrastructure.

## Estrutura

```text
Components/
├── Features/
│   ├── Common/
│   ├── Dashboard/
│   │   ├── Components/
│   │   ├── Pages/
│   │   └── State/
│   ├── Habits/
│   ├── Tasks/
│   ├── Todos/
│   ├── Projects/
│   └── Profile/
├── Layout/
├── Pages/
└── Shared/
```

## Responsabilidades

- **Pages**: rotas, ciclo de vida e navegação.
- **State**: estado da tela, operações e orquestração de UI.
- **Components**: renderização e eventos de interação.
- **Models**: dados temporários de formulários e editores.
- **LevelUpWebService**: adaptação dos estados e componentes aos casos de uso da Application.

## Ciclo de estado

`DashboardState` e `ProfileState` são `Scoped`. Em Blazor Server, isso mantém uma instância por circuito do usuário.

`DashboardModalState` é criado e controlado pelo `DashboardState`, evitando um estado global separado para modais.

## Dependências permitidas

```text
Components/State -> LevelUpWebService -> Application -> Domain
                                           |
                                           v
                                     Infrastructure
```

A camada Web não acessa diretamente o repositório JSON.

## CSS

- CSS específico permanece em `.razor.css`.
- Estilos globais ficam em `wwwroot/css`.
- Arquivos scoped não utilizam `@import`.
