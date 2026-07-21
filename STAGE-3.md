# Etapa 3 — Application

## Entregue

A camada Application foi organizada por feature:

```text
Common/
Features/
├── Dashboard/
├── Profiles/
├── Habits/
├── Tasks/
├── Todos/
└── Projects/
```

## Decisões

- Remoção da antiga fachada extensa `ILevelUpService`.
- Interfaces pequenas e específicas por caso de uso.
- Requests e responses próximos da feature consumidora.
- Leitura do dashboard com response explícita.
- Operações compartilhadas concentradas em `ApplicationService`.
- Registro independente das features na injeção de dependência.
- Adaptação da interface concentrada em `LevelUpWebService`.
