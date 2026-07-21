# Changelog

## Entrega B — Cards

- Adicionados `LevelUpCard` e `LevelUpCardMenu` ao Design System.
- Centralizado o menu Edit/Delete e seu estado de abertura.
- Migrados Habit, Task, To-Do e Project cards para a base reutilizável.
- Consolidado o CSS dos cards em `wwwroot/css/cards.css`.
- Preservadas as regras de pontuação, conclusão, cores e eventos de cada card.

## Etapas 1–4 — Backend e infraestrutura

- Solução reorganizada em `src` e `tests`.
- Domínio encapsulado e validado.
- Application dividida por feature.
- Persistência JSON robusta com backups, recuperação e health check.

## Sprint 5.1 — Fundação do frontend

- Frontend organizado por feature.
- Markup e code-behind separados.
- Editores específicos para os quatro tipos de atividade.
- Tipagem de atividade por enum.
- Estruturas genéricas antigas removidas.

## Sprint 5.2 — Gerenciamento de estado

- Estado do dashboard extraído para `DashboardState`.
- Estado de editores e exclusão extraído para `DashboardModalState`.
- Estado de perfil extraído para `ProfileState`.
- Páginas reduzidas ao ciclo de vida e navegação.
- Registro dos estados ajustado ao ciclo do Blazor Server.

## Delivery C — Feedback & UX

- Added toast notifications and a scoped notification service.
- Added operation loading feedback and dashboard skeletons.
- Consolidated destructive confirmation into the Design System.
- Added success and error feedback to dashboard persistence workflows.

## Delivery D — Theme & Tokens

- Expanded semantic theme tokens.
- Centralized typography, spacing, radius, elevation, focus, motion and layer scales.
- Migrated repeated component styles away from literal colors.
- Added consistent `:focus-visible` and reduced-motion behavior.
- Added common layout and accessibility utilities.

## Delivery E — Frontend tests

- Added `LevelUp.Web.Tests` with bUnit 2.7.2 and xUnit v3.
- Added 37 component test cases for Buttons, Forms, Cards and Feedback.
- Added coverage for accessibility attributes, callbacks, binding, loading, menus, dialogs and toasts.
- Integrated the test project into `LevelUp.slnx` and central package management.

## Delivery E.1 — bUnit corrections

- Added reusable EditContext test infrastructure for form components.
- Corrected explicit boolean ARIA rendering in Button and Card Menu.
- Updated AngleSharp to 1.5.2 to remove NU1902.
- Removed an unnecessary global using from the Web test project.

## Delivery F.1 — Drag & Drop

- Added reusable `LevelUpSortable` behavior with mouse, touch, pen and keyboard support.
- Added visible insertion feedback, drag preview and reduced-motion behavior.
- Added persistent ordering through the Application and Domain layers.
- Preserved filtered-out card positions when reordering search results.
- Added ordering tests for the Domain, Application and Web helper layers.
