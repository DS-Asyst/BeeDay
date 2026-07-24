# Sprint 1.1 — Nova arquitetura do domínio Daily

## Implementado

- Habits permanecem independentes.
- Tasks permanecem independentes e mantêm recorrência `None`, `Daily`, `Weekly` e `Monthly`.
- Projects passam a representar objetivos compostos por zero ou mais To-Dos.
- Todo To-Do exige um `ProjectId` válido.
- Project é o agregado responsável por adicionar, localizar e remover seus To-Dos.
- Progresso, total de To-Dos, total concluído, status e conclusão do Project são calculados.
- Projects vazios são permitidos e ficam com status `Planned` e progresso `0%`.
- Status possíveis: `Planned`, `InProgress` e `Completed`.
- A exclusão de um Project exclui seus To-Dos em cascata.
- Um To-Do pode ser movido entre Projects durante a edição.
- Persistência atualizada para schema 2.
- To-Dos antigos sem vínculo são migrados para o primeiro Project disponível ou para um Project automático chamado `Imported To-Dos`.
- O modal de To-Do recebeu somente o seletor necessário de Project.
- O status manual foi removido do modal de Project.
- O modal de Project recebeu cor e data prevista.

## Observação de validação

O SDK .NET não estava disponível no ambiente utilizado para aplicar as alterações. Portanto, o build e os testes devem ser executados localmente:

```bash
dotnet restore
dotnet build LevelUp.slnx
dotnet test LevelUp.slnx
```
