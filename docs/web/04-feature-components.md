# Feature Components

**Fonte da verdade:** verificado diretamente em `src/BeeDay.Web/Components/Features/` (12 pastas de
área) e `src/BeeDay.Web/Services/BeeDayWebService.cs`.

**Última verificação:** 2026-08-07.

## 1. Objetivo

Descrever cada uma das 12 áreas sob `Components/Features/`: o que cada uma renderiza, que estado
mantém, e como fala com `BeeDay.Application`.

## 2. Visão geral das 12 áreas

| Área | Rota(s) que serve | Componente raiz | State/Model próprio |
|---|---|---|---|
| `Dashboard` | `/daily` | `Pages/Home.razor` | `State/DashboardState.cs`, `DashboardModalState.cs`, `ActivitySortOption.cs` |
| `Wallets` | `/wallet` | `Pages/Wallet.razor` | `State/WalletPageState.cs`, `WalletInteractionState.cs` |
| `Habits` | (modal dentro do Dashboard) | `Components/HabitEditorModal.razor` | `Models/HabitEditorModel.cs`, `HabitVisualState.cs` |
| `Tasks` | (modal dentro do Dashboard) | `Components/TaskEditorModal.razor` | `Models/TaskEditorModel.cs` |
| `Todos` | (modal dentro do Dashboard/Projects) | `Components/TodoEditorModal.razor` | `Models/TodoEditorModel.cs` |
| `Projects` | (modal + workspace dentro do Dashboard) | `Components/ProjectWorkspace.razor`, `ProjectEditorModal.razor` | `Models/ProjectEditorModel.cs` |
| `Account` | `/account`, `/settings` | `Pages/Account.razor` | `Models/ProfileFormModel.cs`, `SecurityFormModel.cs`, `PreferencesFormModel.cs` |
| `Authentication` | `/login` | `Pages/Login.razor` | — (formulário HTML puro, sem model C#) |
| `Identity` | `/account/{confirm-email,reset-password,forgot-password,resend-confirmation,email-confirmation-sent}` | 5 páginas independentes | classes `PasswordForm`/`EmailForm` privadas por página |
| `Onboarding` | `/onboarding/tutorial` | `Pages/Tutorial.razor` | `TutorialSlide` record privado (5 slides hardcoded) |
| `ProfileCreation` | `/`, `/welcome`, `/profile/create` | `Pages/Entry.razor`, `Welcome.razor`, `CreateProfile.razor` | `State/ProfileCreationState.cs`, `Models/ProfileCreationFormModel.cs` |
| `Experience` | (embutido em `ProfileSidePanel`/`Dashboard`) | `Components/ExperienceBar.razor` | `Models/ExperienceViewModel.cs`, `Feedback/BeeDayFeedback*.cs` |
| `Common` | (compartilhado) | — | `ActivityType.cs` (enum `Habit`/`Task`/`Todo`/`Project`, usado por toda a UI de criação) |

## 3. `Dashboard` — a área mais densa

`DashboardState` (`Services`-like, `Scoped`, injetado com `BeeDayWebService` + `ToastService`) é o
estado central de `/daily`: carrega `DashboardResponse` inteiro
(`GetDashboardQuery`), mantém busca + filtro por `ActivityAttribute` + filtro por projeto em
memória (client-side, sem nova query ao Application a cada digitação), e expõe:

- `FilteredHabits`/`FilteredTasks`/`FilteredTodos`/`FilteredProjects` — filtradas por um único
  método privado `Filter<T>` reutilizado com delegates por tipo (evita duplicar o predicado 4
  vezes; comentário no código justifica por que os 4 DTOs de resumo não compartilham interface).
- `DashboardModalState` (objeto próprio, não achatado em `DashboardState`) — controla qual editor
  está aberto (`ActiveEditor: ActivityType?`) e pré-popula o `*EditorModel` correspondente a partir
  do `*Summary` clicado.
- Feedback de XP: `ExecuteExperienceOperationAsync` compara `Profile.TotalExperience` antes/depois
  de uma operação (registrar hábito positivo, completar task/todo) e expõe `LatestExperienceGain` +
  `ExperienceFeedbackVersion` (incrementado a cada ganho, consumido pelo `ExperienceBar` dentro de
  `ProfileSidePanel` para disparar a animação); um `Task.Delay(750)` guardado por número de versão
  limpa o valor depois — se uma nova operação começar antes, o delay antigo não sobrescreve o novo ganho (checagem
  de versão).
- Reordenação: `ReorderHabitsAsync`/`...TasksAsync`/`...TodosAsync`/`...ProjectsAsync` recebem um
  `SortableReorderEvent` (do `BeeDaySortable` JS interop, ver
  [`05-design-system-integration.md`](05-design-system-integration.md)), calculam a nova ordem via
  `SortableOrder.Move` (puro, testável sem DOM) e só chamam `store.ReorderAsync(...)` se a ordem
  resultante realmente mudou.
- Toda mutação passa por `ExecuteAsync`/`SaveEditorAsync`/`DeleteCurrentEditorItemAsync` — um único
  padrão de "trava `IsBusy`, executa, toast de sucesso/erro, libera `IsBusy`" reaproveitado para
  todas as 4 entidades de atividade, com `AnimateRemovalAsync` (delay de 170ms, `RemovingItemId`)
  dando tempo para a animação CSS de saída antes de recarregar a lista.

`Home.razor.cs`: no `OnInitializedAsync`, se `!State.HasProfile`, tenta `State.GetDataAsync()` — se
lançar `InvalidDomainStateException` (usuário autenticado cujo `User` não existe mais no banco,
mesma condição que a antiga checagem `CurrentUser is null` baseada em JSON expressava), redireciona
para `/login`; caso contrário, para `/profile/create`.

`Common/ActivityType.cs` é o enum central (`Habit`, `Task`, `Todo`, `Project`) usado por
`DashboardModalState`, `ActivityFilterBar` (menu "Create") e `BeeDayWebService`-adjacent models —
não confundir com `Domain.Enums` (que tem seus próprios enums por Aggregate); este é puramente de
UI, para decidir qual editor modal está ativo.

## 4. `Wallets`

`Wallet.razor` **não** usa `DashboardState`/`BeeDayWebService` — mantém seu próprio ciclo de
carregamento (`LoadAsync`/`LoadDataAsync`/`RefreshAsync`) via `ISender` direto (ver §8) e dois
objetos de estado dedicados:

- `WalletPageState` — filtros (busca, tipo, tag, intervalo de data), ordenação, paginação
  (`Page`, `ActiveFilterCount` derivado).
- `WalletInteractionState` — trava de operação única (`TryBegin(string operation)`/`End()`),
  equivalente ao `IsBusy` de `DashboardState` mas nomeando a operação em andamento.

Todo filtro muda a página para 1 (`ResetPage()`) e dispara `RefreshAsync()`, que refaz as 3 queries
em paralelo (`Task.WhenAll`: `GetWalletSummaryQuery`, `GetWalletTagsQuery`,
`GetTransactionsQuery`). `RefreshAfterMutationAsync` (chamado após salvar/excluir transação ou tag)
anuncia o novo saldo via um `<div aria-live="polite">` (`_statusAnnouncement`) e dispara um destaque
visual temporário (`_highlightBalance`, 700ms) no card de saldo.

## 5. `Habits`, `Tasks`, `Todos`, `Projects` — editores de atividade

Os 4 modais de edição (`HabitEditorModal`, `TaskEditorModal`, `TodoEditorModal`,
`ProjectEditorModal`) seguem o mesmo esqueleto sobre `EditorModalShell`
(ver [`05-design-system-integration.md`](05-design-system-integration.md)): `Model`
(`[EditorRequired]`), `IsEditing`, `OnSave`/`OnCancel`/`OnDelete`, uma confirmação de exclusão local
(`showDeleteConfirmation`) e um `HandleKeyDown` que fecha a confirmação ou cancela o modal em
`Escape`. `HabitEditorModal` é o único com lógica extra: `TogglePositive`/`ToggleNegative` alternam
`HabitDirection` (`Positive`/`Negative`/`Both`) com base em quais dos dois já estão habilitados, e
`HabitVisualState.GetEditorClass(Model.VisualBalance)` aplica uma de 7 classes CSS conforme a faixa
de saldo (`sky` ≥21, `green` ≥14, `yellow` ≥7, `white` neutro, 3 níveis de `red-*` negativos) — a
mesma tabela é usada por `HabitCard.CardCssClass` no Dashboard, via o mesmo `HabitVisualState`
estático compartilhado.

`ProjectWorkspace.razor` (não um modal — um painel de detalhe) mostra os To-Dos de um projeto
aberto (`DashboardState.OpenProjectId`); `showTodos` alterna a seção sem nova query.

## 6. `Account`

`Account.razor` compõe 3 seções independentes (`ProfileSection`, `SecuritySection`,
`PreferencesSection`, cada uma com seu próprio `IsBusy` local — `_profileBusy`/`_securityBusy`/
`_preferencesBusy`) sob um `IsAnyBusy` agregado usado só para o overlay `BeeDayLoading`. Cada seção
salva via `BeeDayWebService` (`UpdateUserAsync`, `ChangePasswordAsync`, `UpdatePreferencesAsync`) e
mostra toast de sucesso/erro individualmente — uma seção falhando não bloqueia as outras.

## 7. `Authentication`, `Identity`, `Onboarding`, `ProfileCreation`

- `Login.razor`: formulário HTML puro (`<form method="post" action="/auth/login">`), não
  `EditForm` — reflete que o POST vai para o endpoint minimal API de `Program.cs`
  (ver [`01-composition-root.md`](01-composition-root.md) §8), não para um handler Blazor. O
  `onsubmit` inline desabilita o botão via JS vanilla (sem componente/JS interop dedicado) para
  evitar duplo submit.
- `RedirectToLogin.razor` (usado só por `Routes.razor`'s `NotAuthorized`): preserva o path atual
  como `returnUrl`.
- As 5 páginas de `Identity` (`ConfirmEmail`, `ResetPassword`, `ForgotPassword`,
  `ResendConfirmation`, `EmailConfirmationSent`) são todas single-file (markup + `@code` na mesma
  linha em vários casos), cada uma com uma classe de formulário privada minúscula
  (`PasswordForm`/`EmailForm`) validada por `DataAnnotations`. `ResendConfirmation` e
  `EmailConfirmationSent` implementam o mesmo cooldown de 60s via `PeriodicTimer` de forma
  independente (não extraído para um componente/serviço compartilhado) — candidato a duplicação,
  não corrigido nesta Sprint por ser documentação, não refatoração de produção.
- `Tutorial.razor` (`/onboarding/tutorial`): 5 slides estáticos (`TutorialSlide[]` hardcoded no
  `@code`), sem nenhuma leitura de Application até o último slide, quando `NextAsync` chama
  `Store.CompleteOnboardingAsync()` e navega para `/daily`.
- `ProfileCreationState` (`/profile/create`) é a única state class de Feature que atende **dois
  fluxos ao mesmo tempo**: cadastro anônimo completo (`CreateAccountAsync`, sem sessão) e
  completar perfil de um usuário já autenticado sem perfil (`CompleteUserProfileAsync`) —
  `InitializeAsync(hasAuthenticatedSession)` decide qual dos dois ramos preparar. `Entry.razor`
  (`/`) é o único ponto de decisão real de destino pós-autenticação fora do login/`Program.cs` —
  replica a mesma árvore de decisão de `LoginDestinationResolver` (perfil → onboarding → `/daily`)
  para o caso de um usuário já autenticado navegar direto para `/`.

## 8. Acesso direto a `ISender` (desvio do padrão `BeeDayWebService`)

6 páginas injetam `MediatR.ISender` diretamente, contornando `BeeDayWebService`:

| Página | Comandos/Queries enviados diretamente |
|---|---|
| `Wallets/Pages/Wallet.razor` | `EnsureCurrentWalletCommand`, `GetWalletSummaryQuery`, `GetWalletTagsQuery`, `GetTransactionsQuery`, `CreateTransactionCommand`, `UpdateTransactionCommand`, `DeleteTransactionCommand`, `CreateWalletTagCommand`, `UpdateWalletTagCommand`, `DeleteWalletTagCommand` |
| `Identity/Pages/ConfirmEmail.razor` | `ConfirmEmailCommand` |
| `Identity/Pages/ResetPassword.razor` | `ResetPasswordCommand` |
| `Identity/Pages/ForgotPassword.razor` | `RequestPasswordResetCommand` |
| `Identity/Pages/ResendConfirmation.razor` | `ResendEmailConfirmationCommand` |
| `Identity/Pages/EmailConfirmationSent.razor` | `ResendEmailConfirmationCommand` |

Isso contradiz a afirmação em
[`docs/architecture/05-runtime-flows.md`](../architecture/05-runtime-flows.md) §2 ("nenhum
componente Razor injeta `ISender` diretamente — `BeeDayWebService` é o único ponto de acoplamento
confirmado") — ver o achado correspondente em [`README.md`](README.md#achados-relevantes-reportados-não-corrigidos).
Não há indício de que isso seja acidental: os 6 casos são exatamente as páginas cujos
Commands/Queries (Wallet, Identity) nunca foram adicionados a `BeeDayWebService`, que só expõe
métodos para Habits/Tasks/Todos/Projects/Ordering/Users/Onboarding.

## 9. `Experience`

`ExperienceBar.razor` é puramente apresentacional (`ExperienceViewModel`, calcula
`ProgressPercentage` clampado 0-100). O feedback de level-up é o único fluxo Web que reage a um
domain event em vez de a uma resposta direta de comando:

```text
Handler de Application publica UserLeveledUpDomainEvent
  → MediatR entrega a BeeDayFeedbackEventHandler (INotificationHandler<DomainEventNotification>)
  → BeeDayFeedbackStore.Add(...) (deduplicado por ExperienceEntryId via HashSet)
  → BeeDayFeedbackStore.Changed dispara re-render de quem escuta (BeeDayFeedbackHost/Modal)
```

`BeeDayFeedbackStore` mantém um histórico das últimas 3 notificações (`History`), além do
`Current` consumível uma vez (`Consume()`). Este é o mesmo mecanismo referenciado em
[`01-composition-root.md`](01-composition-root.md) §6.

## 10. Fontes de verdade

- Todos os arquivos `.razor`/`.razor.cs`/`.cs` sob `src/BeeDay.Web/Components/Features/` (12
  pastas: `Account`, `Authentication`, `Common`, `Dashboard`, `Experience`, `Habits`, `Identity`,
  `Onboarding`, `ProfileCreation`, `Projects`, `Tasks`, `Todos`, `Wallets`).
- `src/BeeDay.Web/Services/BeeDayWebService.cs`.
