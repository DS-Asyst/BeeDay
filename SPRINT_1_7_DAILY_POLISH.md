# Sprint 1.7 — Polimento do Daily

## Objetivo

Refinar a experiência visual e interativa do Daily sem alterar regras de negócio.

## Alterações aplicadas

- Padronização das transições por meio dos tokens globais de movimento.
- Correção da animação global de entrada, que possuía sintaxe CSS inválida.
- Correção dos estados visuais de exclusão do menu de cards.
- Estados consistentes de `hover`, `active`, `focus-visible` e `disabled`.
- Elevação suave e feedback de foco nos cards de Habit, Task, To-Do e Project.
- Animação de abertura para menus e seções concluídas.
- Scrollbars globais e locais mais finas, com estado de hover alinhado à identidade roxa.
- Melhorias de legibilidade, seleção de texto e renderização tipográfica.
- Revisão do grid responsivo do Daily:
  - quatro colunas no desktop;
  - duas colunas em tablets;
  - uma coluna em dispositivos móveis;
  - remoção da dependência de rolagem horizontal em telas menores.
- Ajustes de espaçamento baseados na escala do Design System.
- Suporte preservado a `prefers-reduced-motion`.
- Melhorias para dispositivos sem hover e interações por toque.

## Escopo preservado

Nenhuma entidade, caso de uso, contrato de aplicação, persistência ou regra de conclusão foi alterada.
