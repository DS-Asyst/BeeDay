# UX Guidelines

**Fonte da verdade:** derivado da leitura direta de `src/BeeDay.Web/Components/**/*.razor(.cs)` e
`src/BeeDay.Web/wwwroot/css/*.css` feita nesta Sprint (16.7 e 16.8) — cada padrão abaixo cita o
arquivo onde foi observado. Não é uma lista de recomendações novas: é uma descrição do que o código
atual faz de forma consistente (ou não).

**Última verificação:** 2026-08-11 (Sprint 20.5, EPIC 20) — §5 atualizado: `/` deixou de ser um
resolvedor de redirect e passou a ser a Home pública; demais seções preservadas da verificação de
2026-08-07.

## 1. Objetivo

Descrever os padrões de experiência do usuário que se repetem através do produto: como a
hierarquia visual é comunicada, como o sistema dá feedback de uma ação, o que acontece em cada
estado (vazio, carregando, erro), e onde o produto pede confirmação antes de agir.

## 2. Hierarquia visual

A hierarquia é comunicada principalmente por **fonte**, não só por tamanho: `typography-policy.css`
reserva Jersey 25 (fonte "retro"/pixel) exclusivamente para título de página/card, marca
(`BeeDayBrand`) e botões (`BeeDayButton`) — todo o resto (parágrafos, labels, inputs, tabelas,
valores numéricos) usa Inter. Isso significa que, em qualquer tela, os elementos em Jersey 25 são,
por definição, os pontos de maior peso visual — não é preciso variar tamanho de fonte para
comunicar "isto é o título desta seção".

O padrão estrutural de cabeçalho (`BeeDayPageHeader`/`BeeDaySectionHeader`/`BeeDayHero`, ver
[`docs/design-system/02-components.md`](../design-system/02-components.md) §5) é: eyebrow (rótulo
pequeno, maiúsculo, cor de marca) → título (H1/H2, Jersey 25) → descrição (Inter, cor muted) →
ações (à direita em telas largas, empilhadas abaixo em `max-width: 42rem`). Esse padrão se repete
em `Account.razor` (via `BeeDayPageHeader`) e nas páginas de catálogo — não foi adotado por
`Wallet.razor`/`Home.razor`, que usam markup de cabeçalho próprio (`<header class="wallet-page__header">`),
uma inconsistência estrutural entre as duas telas de produto mais usadas e o componente que existe
especificamente para esse papel.

## 3. Feedback de ação

Três canais distintos, cada um para uma classe diferente de evento:

| Canal | Componente | Quando |
|---|---|---|
| Toast | `BeeDayToastHost`/`ToastService` | Resultado de uma mutação (sucesso ou erro) — sempre a última coisa que acontece após salvar/excluir |
| Overlay de loading | `BeeDayLoading` | Uma mutação está em andamento |
| Anúncio ao vivo silencioso | `<div aria-live="polite">` | Mudança de estado que não é um toast mas deve ser lida por leitor de tela (ex.: `Wallet.razor`'s `_statusAnnouncement` após atualizar o saldo) |

O overlay de loading tem um detalhe deliberado: `feedback.css`'s `.beeday-loading-overlay` usa
`animation: beeday-loading-reveal .16s ease .35s forwards` — **350ms de atraso antes de aparecer**.
Uma operação que termina em menos de 350ms nunca mostra o overlay; isso evita o "flash" de loading
em operações rápidas, um padrão de UX deliberado (não um efeito colateral de performance).

Toasts não persistem: sucesso/info somem em 4s, erro em 7s (`ToastService.Show`), sem exigir
interação — mas têm botão de fechar manual (`aria-label="Dismiss notification"`) para quem quiser
descartar antes.

## 4. Consistência — onde existe e onde não existe

**Existe:** o par `IsBusy`/`Disabled` no mesmo booleano em todo formulário `EditForm` observado —
nenhum formulário do repositório desabilita um botão de submit sem também estar em estado de
carregamento visível, e vice-versa (ver
[`docs/design-system/04-forms.md`](../design-system/04-forms.md) §6). O uso de
`BeeDayConfirmDialog` é exclusivo para ações destrutivas (exclusão) — nenhuma ação não-destrutiva
do repositório passa por uma confirmação.

**Não existe (achados, não corrigidos):**
- 4 implementações CSS independentes do mesmo padrão visual de campo de formulário
  (`.beeday-field__control`, `.editor-modal__field input`, `.identity-field input`,
  `.wallet-filters input`) — ver [`docs/design-system/04-forms.md`](../design-system/04-forms.md) §5.
- `Wallet.razor`/`Home.razor` não usam `BeeDayPageHeader` (ver §2 acima).
- 70 queries de largura em 33 stylesheets (26 cortes `max-width`, dois `min-width`), sem token
  artificial; shell 1200px e famílias públicas compartilhadas estão formalizados — ver
  [`03-responsive.md`](03-responsive.md).

## 5. Fluxo — do primeiro acesso ao Dashboard

```mermaid
flowchart TD
    Home["/ (Home.razor, PublicLayout) — Home pública, sem redirect"] -->|anônimo, CTA \"Get started\"| Login["/login"]
    Home -->|autenticado, CTA \"Continue to BeeDay\"| EntryResolver["AuthenticatedEntryDestinationResolver"]
    EntryResolver --> CreateProfile["/profile/create"]
    EntryResolver --> Tutorial["/onboarding/tutorial"]
    EntryResolver --> Daily["/daily"]

    Login -->|POST /auth/login bem-sucedido| Resolver["LoginDestinationResolver.Resolve"]
    Resolver --> CreateProfile
    Resolver --> Tutorial
    Resolver --> Daily

    CreateProfile -->|conta anônima nova| ConfirmSent["/account/email-confirmation-sent"]
    CreateProfile -->|sessão já autenticada, completando perfil| Tutorial

    Tutorial -->|5 slides, ENTER DAILY no último| Daily
```

**Atualizado na Sprint 20.5 (EPIC 20):** até então, `/` era resolvida por `Entry.razor`, que
replicava a árvore de decisão (perfil → onboarding → destino) de forma independente — 3 cópias
paralelas da mesma regra. `Entry.razor` foi removido; `/` agora é a Home pública (`Home.razor`,
sem nenhum redirect automático). A regra de destino continua vivendo em
`LoginDestinationResolver.Resolve` (`Program.cs`, pós-login) e em `CreateProfile.razor.cs`
(pós-conclusão de perfil) — mas o antigo terceiro consumidor foi substituído por
`AuthenticatedEntryDestinationResolver` (`Services/Authentication/`), que **reutiliza**
`LoginDestinationResolver.Resolve` em vez de reimplementá-lo, para o CTA de um usuário autenticado
que visita `/` e escolhe continuar. Ver
[`docs/web/04-feature-components.md`](../web/04-feature-components.md) e
[`docs/web/02-routing-and-pages.md`](../web/02-routing-and-pages.md) §8.

## 6. Microinterações

`pixel-ui.css` e `animations.css` definem o vocabulário de movimento do produto:

| Interação | Efeito | Onde |
|---|---|---|
| Hover de card | `translateY(-2px/-3px)` + elevação de sombra | Todo card interativo (`beeday-card--interactive`, `.activity-card:hover`, `.habit-card:hover`) |
| Press de botão | `translateY(4px)` — simula um botão físico "afundando" | `BeeDayButton` padrão |
| Entrada de card | `beeday-card-enter` (fade + scale sutil) | Novo item adicionado a uma lista |
| Saída de card | `beeday-card-leave` (fade + colapso de altura) | Item excluído — dá 160ms antes de a lista recalcular |
| Reordenação | `beeday-reorder-settle` (scale .985→1.008→1) | Após soltar um card arrastado no novo lugar |
| Sucesso | `beeday-success-pulse` (brilho + scale) | Disponível como utilitário; consumidor específico não confirmado nesta auditoria |
| Erro de validação | `beeday-shake` (4 passos horizontais) | Disponível como utilitário; consumidor específico não confirmado nesta auditoria |
| Ganho de XP | Overlay de feedback dirigido por domain event (`UserLeveledUpDomainEvent`) | Ver [`docs/web/04-feature-components.md`](../web/04-feature-components.md) §9 |

Toda animação acima é desativada sob `prefers-reduced-motion: reduce` — ver
[`02-accessibility.md`](02-accessibility.md) §5.

## 7. Estados vazios

`BeeDayEmptyState` (Título + Descrição + ícone opcional) é o componente genérico, mas cada coluna
do Dashboard (`DashboardColumn.razor`) gera seu próprio texto de estado vazio a partir de
`EmptyLabel` (ex. "No completed tasks", "Completed tasks will appear here") em vez de compor
`BeeDayEmptyState` diretamente — o padrão visual é o mesmo, a composição não. `WalletEmptyState`
(`Features/Wallets/Components/`) é uma implementação própria, não confirmada nesta auditoria como
reutilizando `BeeDayEmptyState`.

## 8. Estados de carregamento

Dois padrões distintos, para dois momentos diferentes do ciclo de vida de uma página:

1. **Carregamento inicial de página** — skeleton estrutural (`BeeDayDashboardSkeleton` em
   `Home.razor`; um `<BeeDaySkeleton Lines="N">` mais simples em `Account.razor`/`Wallet.razor`),
   visível enquanto a primeira query ainda não voltou.
2. **Mutação em andamento** — `BeeDayLoading` (overlay com atraso de 350ms, ver §3), nunca
   substitui o conteúdo já renderizado — a tela permanece interativa/legível, só o botão de ação
   fica desabilitado.

Não há um terceiro padrão para "recarregando dados já carregados" (ex.: aplicar um filtro no
Wallet) além de `_isRefreshing`/`IsRefreshing` controlando opacidade parcial da lista
(`TransactionList.IsRefreshing`, não auditado em detalhe de CSS nesta Sprint).

## 9. Estados de erro

| Origem do erro | Como é mostrado |
|---|---|
| Falha ao carregar dados (query) | Banner inline (`role="alert"`, ex. `.wallet-alert`/`.identity-feedback--error`) com botão "Try again" quando aplicável |
| Falha ao salvar (mutação) | Toast de erro (`ToastService.ShowError`) +, em alguns formulários, também um banner inline com a mesma mensagem (duplicação deliberada — ex. `Wallet.razor.SaveTransactionAsync` seta `_errorMessage` **e** chama `Toast.ShowError`) |
| Erro de validação de campo | `BeeDayValidationMessage`/`ValidationMessage` inline, abaixo do campo |
| Erro não tratado no servidor | `GlobalExceptionHandler` → `ProblemDetails` — não chega a uma página Blazor visível (ver [`docs/web/01-composition-root.md`](../web/01-composition-root.md) §7); no circuito Blazor, uma exceção não capturada localmente derruba o circuito e aciona `ReconnectModal` |

A mensagem de erro é sempre genérica para o usuário ("We could not load your wallet"), nunca expõe
a exceção técnica — mesmo padrão usado pelo `GlobalExceptionHandler` no servidor (mensagem técnica
só quando `IsDevelopment()`).

## 10. Confirmações

`BeeDayConfirmDialog` é usado exclusivamente antes de excluir: Habit, Task, Todo, Project (via
`EditorModalShell.OnDelete`), Transaction, WalletTag. Estrutura fixa: título, pergunta, nome do
item entre aspas quando disponível (`ItemTitle`), aviso opcional com detalhe (`Warning`/
`WarningDetails`), dois botões (`ConfirmationCancel`/`ConfirmationDanger`). Fecha com `Escape`,
clique no backdrop, ou qualquer um dos dois botões — nunca fecha sozinho.

Duas telas usam a mesma mensagem neutra deliberadamente, por razão de segurança, não de UX pura:
`ForgotPassword.razor` e `ResendConfirmation.razor` mostram "If an account exists for this
email, ..." independentemente de o e-mail existir ou não — evita que a resposta da UI revele quais
e-mails têm conta.

## 11. Fontes consultadas

- `src/BeeDay.Web/wwwroot/css/feedback.css`, `pixel-ui.css`, `animations.css`,
  `typography-policy.css`, `design-system.css`.
- `src/BeeDay.Web/Components/DesignSystem/Feedback/*`, `Layout/BeeDayPageHeader.razor.cs`.
- [`docs/web/04-feature-components.md`](../web/04-feature-components.md),
  [`docs/web/01-composition-root.md`](../web/01-composition-root.md) (Sprint 16.7, reaproveitado).
- [`docs/design-system/02-components.md`](../design-system/02-components.md),
  [`04-forms.md`](../design-system/04-forms.md) (Sprint 16.8).
