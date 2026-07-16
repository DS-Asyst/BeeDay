# Persistência

## Formato

O LevelUp persiste o estado localmente em JSON. O arquivo contém dados pessoais e financeiros reais e não deve ser versionado.

## Versionamento

`GameData.SchemaVersion` identifica o formato do save. `GameData.CurrentSchemaVersion` representa a versão mais recente.

O carregamento segue:

1. desserializar;
2. normalizar coleções ausentes;
3. detectar versão;
4. executar migrações sequenciais;
5. validar integridade;
6. criar os serviços da sessão.

Migrações ficam em `Services/Persistence/Migrations` e não devem ser executadas pela UI ou por `Program.cs`.

## Escrita atômica

O salvamento escreve primeiro em `save.json.tmp`, força o flush em disco, preserva o snapshot anterior em `save.json.previous` e somente então substitui o arquivo principal.

## Validação

`GameDataValidator` verifica:

- IDs duplicados;
- missões apontando para projetos ou capítulos inexistentes;
- capítulos e Chefes sem projetos válidos;
- progresso inválido de livros.

## Corrupção

JSON inválido gera um backup datado e `CorruptedSaveException`. O backup também contém dados pessoais e deve receber a mesma proteção do save principal.
