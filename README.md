# LevelUp

Aplicação console em .NET 10 que transforma hábitos, missões, projetos, leitura e finanças pessoais em um sistema de progressão inspirado em RPG.

## Estado atual

A Fase 9 introduz persistência operacional com SQLite e Entity Framework Core. O domínio permanece independente da infraestrutura e o console passa a ser um cliente separado.

## Projetos

- `LevelUp`: domínio, aplicação, workflows, serviços e UI compartilhada;
- `LevelUp.Infrastructure`: SQLite, EF Core, migrations e repositórios;
- `LevelUp.Console`: executável e composition root;
- `LevelUp.Tests`: testes automatizados.

## Pré-requisitos

- .NET SDK 10;
- Git;
- opcional: DB Browser for SQLite ou o comando `sqlite3` para inspeção manual.

O `dotnet-ef` é fornecido por manifesto local e não precisa ser instalado globalmente.

## Preparação

```bash
dotnet restore
dotnet tool restore
dotnet build
dotnet test
```

## Executar

```bash
dotnet run --project LevelUp.Console
```

Na primeira execução, a aplicação cria o banco automaticamente. Se não houver personagem persistido, o fluxo de criação do primeiro personagem é iniciado. Nenhum arquivo JSON é importado.

## Local do banco

Windows:

```text
%LOCALAPPDATA%\LevelUp\levelup.db
```

Para usar outra pasta:

Git Bash:

```bash
export LEVELUP_DATA_DIR="/c/DevOps/LevelUpData"
dotnet run --project LevelUp.Console
```

PowerShell:

```powershell
$env:LEVELUP_DATA_DIR = "C:\DevOps\LevelUpData"
dotnet run --project LevelUp.Console
```

## Inspecionar o SQLite

Com `sqlite3` instalado:

```bash
sqlite3 "$LOCALAPPDATA/LevelUp/levelup.db"
.tables
SELECT * FROM GameMetadata;
SELECT Id, UpdatedAtUtc FROM Books ORDER BY Id;
.quit
```

Também é possível abrir `levelup.db` no DB Browser for SQLite.

## Migrations

```bash
dotnet ef migrations list --project LevelUp.Infrastructure --startup-project LevelUp.Console
dotnet ef database update --project LevelUp.Infrastructure --startup-project LevelUp.Console
```

Nova migration:

```bash
dotnet ef migrations add NomeDaMigration \
  --project LevelUp.Infrastructure \
  --startup-project LevelUp.Console \
  --output-dir Persistence/Migrations
```

## Persistência exclusiva

O arquivo `%LOCALAPPDATA%\LevelUp\levelup.db` é a única fonte de verdade da aplicação. Não existe importação automática, exportação automática ou fallback para arquivos JSON. Atualizações de estrutura devem ser feitas por migrations do EF Core; ajustes controlados de dados podem ser realizados por scripts SQLite.

Os arquivos `.db`, `-wal` e `-shm` são dados locais do usuário e permanecem fora do Git.

## Validação antes do merge

```bash
dotnet format --verify-no-changes
dotnet build -c Release
dotnet test -c Release
git status
```
