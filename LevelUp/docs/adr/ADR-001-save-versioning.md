# ADR-001 — Versionamento do save

## Status

Aceito.

## Decisão

Todo `GameData` possui `SchemaVersion`. Mudanças incompatíveis exigem migrações incrementais executadas no carregamento antes da validação.

## Consequências

Saves antigos continuam utilizáveis, o bootstrap fica livre de compatibilidade e cada mudança de schema precisa de testes.
