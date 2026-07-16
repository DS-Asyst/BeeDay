# ADR-003 — Composição da aplicação

## Status

Aceito.

## Decisão

`ApplicationBootstrap` compõe serviços e telas. `GameSession` agrupa o estado da sessão. `Program.cs` apenas inicializa e executa a aplicação.

## Consequências

A inicialização fica testável e a futura migração para DI ou Blazor exige menos alterações.
