# Estratégia de testes

## Prioridades

1. transições de estado do domínio;
2. workflows que alteram mais de um módulo;
3. persistência, migração e validação;
4. consultas analíticas;
5. componentes de texto e cancelamento de entrada.

## Cobertura

A cobertura é coletada por:

```bash
dotnet test LevelUp.Tests/LevelUp.Tests.csproj \
  --collect:"XPlat Code Coverage" \
  --settings coverlet.runsettings
```

A meta inicial é 75% global, sem perseguir cobertura artificial. Workflows críticos e migrações devem ter cobertura integral de cenários relevantes.

## Convenções

- testes não gravam no diretório real da aplicação;
- datas devem ser fixas;
- testes de persistência usam diretórios temporários;
- uma falha deve indicar a regra de negócio quebrada, não detalhes da UI.
