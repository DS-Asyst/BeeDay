# Localization

**Fonte da verdade:** verificado diretamente em `src/BeeDay.Web/Localization/`,
`src/BeeDay.Web/Program.cs`, `src/BeeDay.Web/Services/Authentication/AuthenticatedCultureSynchronizer.cs`,
os catálogos `.resx` sob `src/BeeDay.Web/` e `tests/BeeDay.Web.Tests/Localization/`. Construído pela
EPIC 23 (Sprints 23.1–23.9); nenhuma afirmação vem de sprints anteriores sem reverificação direta no
código.

**Última verificação:** 2026-08-15 (Sprint 23.9, EPIC 23 — sprint de consolidação final).

## 1. Objetivo

Descrever como o BeeDay resolve, persiste e aplica o idioma de apresentação (en-US/pt-BR): a fonte
de verdade da cultura efetiva por requisição, a regra de precedência entre cookie explícito e
preferência de conta, o endpoint que grava a escolha, a convenção de organização dos catálogos de
recurso e a responsabilidade da camada Web por nunca deixar uma frase crua de `Domain`/`Application`
chegar ao usuário.

## 2. Escopo

Dentro: `src/BeeDay.Web/Localization/` (`BeeDayCultures`, `AuthenticatedAccountCultureProvider`),
`AuthenticatedCultureSynchronizer`, o endpoint `POST /culture/set`, a convenção de catálogos
`.resx`/`IStringLocalizer<T>`, e os dois tradutores Web-only que impedem mensagens cruas de
Domain/Application e de DataAnnotations de vazar para a UI (`DomainErrorLocalizer`,
`ValidationMessageLocalizer`). Fora: o conteúdo textual de cada catálogo individual (ver o próprio
`.resx` da feature); detalhe de `Program.cs` fora do bloco de localização (ver
[`01-composition-root.md`](01-composition-root.md)); `User.Language`/`UserTheme` como Value Object
de Domain (ver [`docs/domain/user.md`](../domain/user.md)).

## 3. Culturas suportadas e cultura padrão

`BeeDayCultures` (`src/BeeDay.Web/Localization/BeeDayCultures.cs`) é a única fonte de verdade para
quais culturas existem, qual é o fallback e o nome do cookie:

```csharp
public const string English = "en-US";
public const string Portuguese = "pt-BR";
public const string Default = English;

public const string CookieName = "BeeDay.Culture";

public static readonly string[] Supported = [English, Portuguese];
```

**Cultura padrão: `en-US`.** Uma requisição anônima sem cookie `BeeDay.Culture` e sem sessão
autenticada sempre resolve para inglês — não há sniffing de `Accept-Language` do navegador (ver §4).

`FromUserLanguage(UserLanguage)`/`ToUserLanguage(string)` fazem a única conversão entre o enum de
Domain `UserLanguage` (`English`/`Portuguese`, ver [`docs/domain/user.md`](../domain/user.md)) e o
código de cultura `en-US`/`pt-BR` usado pela Web — mantendo o Domain livre de qualquer conceito de
cultura .NET (`CultureInfo`, `RequestLocalization`), consistente com a regra de dependência descrita
em [`docs/architecture/03-clean-architecture.md`](../architecture/03-clean-architecture.md).

## 4. Pipeline de resolução de cultura

Registrado em `Program.cs`:

```csharp
builder.Services.AddLocalization();
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.SetDefaultCulture(BeeDayCultures.Default)
        .AddSupportedCultures(BeeDayCultures.Supported)
        .AddSupportedUICultures(BeeDayCultures.Supported);

    options.RequestCultureProviders =
    [
        new CookieRequestCultureProvider { CookieName = BeeDayCultures.CookieName },
        new AuthenticatedAccountCultureProvider(builder.Environment.IsDevelopment())
    ];
});
```

Ordem no pipeline HTTP (ver [`01-composition-root.md`](01-composition-root.md) §7):

```text
UseAuthentication() → UseAuthorization() → UseRequestLocalization() → UseAntiforgery()
```

`UseAuthentication()` roda **antes** de `UseRequestLocalization()` deliberadamente: o segundo
provider (`AuthenticatedAccountCultureProvider`) precisa que o `User` autenticado já tenha sido
carregado e depositado em `HttpContext.Items` pelo evento `OnValidatePrincipal` do cookie de
autenticação, para poder ler `User.Language` sem uma segunda consulta ao repositório. Requisições
anônimas não são afetadas — `HttpContext.Items` simplesmente não carrega nada, e o segundo provider
devolve `null`.

`CultureInfo.CurrentCulture` (formatação de número/data) e `CurrentUICulture` (resolução de `.resx`)
são sempre definidos juntos para o mesmo valor — `RequestLocalizationMiddleware` nunca separa "c="
de "uic=" no valor do cookie `BeeDay.Culture`.

## 5. Precedência: cookie explícito → `User.Language` → fallback

A regra, válida em toda a aplicação, é aplicada por três mecanismos concretos que nunca se
sobrepõem — cada um cobre exatamente o caso que os outros dois não alcançam:

| # | Mecanismo | Quando roda | O que faz |
|---|---|---|---|
| 1 | `CookieRequestCultureProvider` (built-in ASP.NET Core) | Toda requisição, primeiro provider da lista | Se o cookie `BeeDay.Culture` existe e é válido, **sempre vence** — os outros dois nunca são sequer consultados. |
| 2 | `AuthenticatedAccountCultureProvider` (`src/BeeDay.Web/Localization/AuthenticatedAccountCultureProvider.cs`) | Toda requisição autenticada sem cookie explícito ainda | Lê `User.Language` (via `HttpContext.Items`, sem nova consulta) e resolve a cultura a partir dele — cobre a sessão "lembrar de mim" que nunca repassa por `POST /auth/login` (reabrir o navegador dias depois). Também grava o cookie na resposta, para que a próxima requisição já resolva pelo provider 1. |
| 3 | `AuthenticatedCultureSynchronizer` (`src/BeeDay.Web/Services/Authentication/AuthenticatedCultureSynchronizer.cs`) | Uma vez, dentro de `POST /auth/login`, logo após `SignInAsync` | Sem cookie explícito: aplica `User.Language` e grava o cookie. Com cookie explícito que diverge de `User.Language`: o cookie vence **nesta sessão** e a conta converge para ele via `UpdateCurrentUserPreferencesCommand`, para que sessões futuras já nasçam sincronizadas. |

Não há, em nenhum ponto, sniffing de `Accept-Language` do navegador — resolvido deliberadamente fora
do escopo até que uma necessidade concreta justifique adicionar esse provider.

## 6. Endpoint `POST /culture/set`

Único mecanismo oficial de troca de idioma (`Program.cs`, detalhado em
[`01-composition-root.md`](01-composition-root.md) §8). Recebe `culture` (rejeitado com
`400 Bad Request` se fora de `BeeDayCultures.Supported`) e `returnUrl?` opcional, grava o cookie
`BeeDay.Culture` e redireciona. Dois pontos da aplicação postam para ele, ambos com
`<AntiforgeryToken />`:

- **Área pública** — `PublicLanguageSwitcher.razor` (`Components/Layout/`), visível em `Home`/`Login`/
  `CreateAccount` etc. para um visitante ainda anônimo.
- **Settings** — o formulário oculto `#culture-sync-form` em
  `Components/Features/Account/Pages/Account.razor`: ao salvar preferências com um `Language`
  diferente do atual, `SaveAsync` chama `SyncCultureAndReloadAsync`, que envia esse formulário em vez
  de apenas mostrar um toast — o valor já foi persistido em `User.Language` via
  `UpdateCurrentUserPreferencesCommand` antes disso; o POST para `/culture/set` sincroniza o cookie e
  recarrega a página para que a UI já renderize no novo idioma imediatamente, sem exigir um segundo
  login.

## 7. Sessão persistente

Um usuário autenticado que nunca definiu o cookie `BeeDay.Culture` explicitamente (sessão iniciada
antes da EPIC 23, ou aberta via cookie "lembrar de mim" sem nunca ter passado pela troca manual de
idioma) continua recebendo `User.Language` como cultura efetiva a cada requisição, via o provider 2
da tabela acima — não exige um novo login. Este é o comportamento coberto desde a Sprint 23.3 e
reverificado nesta Sprint (23.9) sem alteração.

## 8. Convenção de catálogos de recurso

Um catálogo por área — tipicamente a pasta raiz de uma feature Web — nunca um catálogo global único
por conveniência:

```text
<Área>/
├── <Área>Resources.cs           marker class vazia (âncora de IStringLocalizer<T>)
├── <Área>Resources.resx          neutro — usado como fallback se a cultura não resolver
├── <Área>Resources.en-US.resx    conteúdo idêntico ao neutro nesta base de código
└── <Área>Resources.pt-BR.resx    tradução
```

16 catálogos existem hoje (`find src/BeeDay.Web -name "*.resx" ! -name "*.en-US.resx" ! -name "*.pt-BR.resx"`):

`SharedResources` (`Resources/`, cross-cutting — toasts genéricos, rodapé, 404, e as mensagens de
`DomainErrorLocalizer`, ver §9), `LayoutResources`, `DesignSystemResources` (inclui as mensagens de
`ValidationMessageLocalizer`, ver §9), `AccountResources`, `AuthenticationResources`,
`DashboardResources`, `ExperienceResources`, `HabitResources`, `HomeResources`, `IdentityResources`,
`OnboardingResources`, `ProfileCreationResources`, `ProjectResources`, `TaskResources`,
`TodoResources`, `WalletResources`.

Regras observadas de forma consistente em todos os catálogos (auditadas nesta Sprint — zero exceção
encontrada):

- Paridade obrigatória: toda chave do `.resx` neutro existe em `.en-US.resx` e `.pt-BR.resx`, e
  vice-versa.
- Chaves resolvidas por indexador em runtime (`Localizer["Key"]`), nunca por classe `Designer.cs`
  gerada — não há `ErrorMessageResourceType`/`ErrorMessageResourceName` neste código-base.
- Conteúdo gerado pelo usuário (título de hábito/tarefa/projeto, notas, nome de tag) nunca é
  localizado — é dado, não texto de interface.
- Valores técnicos/ISO (`yyyy-MM-dd` de `<input type="date">`, `datetime` de `<time>`, correlation
  ID) nunca são localizados.
- Um componente cujo parâmetro já tem um default localizado em outro catálogo (ex.: os rótulos
  Confirmar/Cancelar de `BeeDayConfirmDialog`, em `DesignSystemResources`) não duplica a chave no
  catálogo da feature que o consome — só declara chave própria para o que efetivamente sobrescreve.

## 9. Responsabilidade da Web pelas mensagens localizadas

`Domain` e `Application` permanecem inteiramente livres de `IStringLocalizer`/`CultureInfo` —
lançam e retornam texto em inglês simples, sem qualquer conceito de cultura (consistente com
[`docs/architecture/03-clean-architecture.md`](../architecture/03-clean-architecture.md) e
[`docs/application/05-exceptions.md`](../application/05-exceptions.md)). É `BeeDay.Web`, e somente
ela, que interpreta esse texto em inglês e decide a representação localizada — nunca o contrário.

Dois tradutores, ambos em `src/BeeDay.Web/Localization/`, fazem essa ponte sem introduzir
`IStringLocalizer` em nenhuma camada interna:

- **`DomainErrorLocalizer`** — casa `InvalidDomainStateException`/`DomainValidationException`/
  `ApplicationValidationException` (por tipo e, para `InvalidDomainStateException`, por trecho de
  mensagem conhecido) contra chaves de `SharedResources`. Mensagem não reconhecida cai num fallback
  genérico localizado — nunca no texto cru. Usado nos pontos que antes exibiam `exception.Message`
  diretamente (Account/Settings, criação de perfil, reset de senha, reenvio de confirmação).
- **`ValidationMessageLocalizer`** — dicionário de correspondência exata entre o texto literal de
  `ErrorMessage` das anotações de `DataAnnotations` (`[Required]`, `[StringLength]`, `[Compare]`,
  `[RegularExpression]`, `[EmailAddress]`) e chaves de `DesignSystemResources`. Conectado ao
  componente `BeeDayValidationMessage` (`Components/DesignSystem/Forms/`), que é o único ponto por
  onde `BeeDayInput`/`BeeDayTextArea`/`BeeDaySelect`/`BeeDayCheckbox`/`BeeDayDateInput` renderizam
  mensagens de validação — corrigir esse componente único cobre a validação de todo formulário do
  produto sem qualquer alteração nos `*EditorModel.cs` ou nos validators.

Nenhum dos dois tradutores existe em `BeeDay.Domain`/`BeeDay.Application`/`BeeDay.Infrastructure` —
verificável por grep de `IStringLocalizer`/`ResourceManager` fora de `BeeDay.Web` (0 ocorrências).

## 10. Datas e números

Formatação de data visível ao usuário usa os especificadores padrão do .NET
(`DateOnly.ToString("d")`/`DateTime.ToString("d")`, controlados por `CultureInfo.CurrentCulture`),
nunca um formato customizado como `"MMM dd, yyyy"` — um formato customizado fixa a ordem
dia/mês/ano independentemente da cultura; só o valor do nome do mês mudaria, não a estrutura. Isso
é o que faz `en-US` (mês/dia/ano) e `pt-BR` (dia/mês/ano) genuinamente divergirem em estrutura, não
só em idioma. Valores ISO (`yyyy-MM-dd` de `<input type="date">`/`<time datetime="...">`,
persistência) permanecem sempre ISO, nunca convertidos.

## 11. Testes

`BunitLocalizationSupport` (`tests/BeeDay.Web.Tests/Localization/BunitLocalizationSupport.cs`)
fixa `CultureInfo.CurrentCulture` e `CurrentUICulture` juntos ao redor de um render/ação
(`WithUiCulture`/`WithUiCultureAsync`) e é obrigatório em todo teste que faça asserção sobre texto
culture-sensível: **a máquina de desenvolvimento usa `pt-BR` como cultura ambiente por padrão**, não
`en-US` — um teste que assere texto em inglês sem fixar a cultura explicitamente passa
silenciosamente na CI (ambiente en-US) e falha localmente, ou o inverso. Toda `BunitContext` que
renderiza um componente injetando `IStringLocalizer<T>` precisa registrar `Services.AddLogging()` e
`Services.AddLocalization()`.

## 12. Fontes de verdade

- `src/BeeDay.Web/Localization/BeeDayCultures.cs`, `AuthenticatedAccountCultureProvider.cs`,
  `DomainErrorLocalizer.cs`, `ValidationMessageLocalizer.cs`
- `src/BeeDay.Web/Services/Authentication/AuthenticatedCultureSynchronizer.cs`
- `src/BeeDay.Web/Program.cs` (bloco `AddLocalization`/`RequestLocalizationOptions`, ordem do
  pipeline, endpoint `/culture/set`)
- `src/BeeDay.Web/Components/DesignSystem/Forms/BeeDayValidationMessage.razor(.cs)`
- Os 16 catálogos `.resx`/`.en-US.resx`/`.pt-BR.resx` sob `src/BeeDay.Web/`
- `tests/BeeDay.Web.Tests/Localization/BunitLocalizationSupport.cs`,
  `DomainErrorLocalizerTests.cs`, `ValidationMessageLocalizerTests.cs`
- `tests/BeeDay.Web.Tests/Integration/IdentityFlowLocalizationIntegrationTests.cs`,
  `CultureCookieIntegrationTests.cs`, `AuthenticatedCultureIntegrationTests.cs`
