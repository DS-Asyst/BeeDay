# Estratégia de testes

## Objetivos

A suíte deve proteger regras de domínio, workflows, persistência e compatibilidade de dados.

## Categorias

- `Domain`: invariantes das entidades;
- `Features`: comportamento por funcionalidade;
- `Rewards`: regressões de recompensa e progressão;
- `Infrastructure`: SQLite, migrations, round-trip e importação;
- `UI`: formatação e leitura de entrada sem regras de negócio.

## Persistência SQLite

Os testes devem usar diretórios temporários e bancos isolados. Nenhum teste pode depender do banco real em `%LOCALAPPDATA%`.

Cobertura mínima:

1. banco vazio retorna ausência de jogo;
2. salvar e carregar preserva o snapshot;
3. gravação gera backup JSON;
4. migration inicial cria todas as tabelas;
5. importação do JSON acontece apenas em banco vazio;
6. falha durante a transação não deixa snapshot parcial;
7. IDs e relacionamentos inválidos continuam bloqueados pelo `GameDataValidator`.

## Execução

```bash
dotnet restore
dotnet tool restore
dotnet format --verify-no-changes
dotnet build
dotnet test
```

Antes de merge em `develop`, executar também:

```bash
dotnet build -c Release
dotnet test -c Release
```
