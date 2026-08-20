using System.Text.RegularExpressions;
using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace BeeDay.E2E.Tests;

/// <summary>
/// Wallet access, tag creation, and transaction creation as seen in the browser, ending with the
/// visible balance actually reflecting the new transaction.
/// </summary>
public sealed class WalletTests(PlaywrightAppFixture fixture) : E2ETestBase(fixture)
{
    private const string Password = "E2ePassword123!";

    [Fact]
    public async Task CreateTagAndTransaction_UpdatesBalance()
    {
        var email = $"e2e-wallet-{Guid.NewGuid():N}@beeday.invalid";
        await Fixture.Factory.SeedUserAsync(email, Password, onboardingCompleted: true);

        await GotoAsync("/login");
        await Page.GetByLabel("Email").FillAsync(email);
        await Page.GetByLabel("Password").FillAsync(Password);
        await Page.GetByRole(AriaRole.Button, new() { Name = "Sign In" }).ClickAsync();
        await Expect(Page).ToHaveURLAsync(new Regex("/profile$"));
        await GotoAsync("/daily");
        // See AccountLifecycleTests.LoginAsync's remarks: a redirect-triggered navigation
        // establishes its own SignalR circuit that GotoAsync's own wait cannot cover.
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await Page.GetByRole(AriaRole.Link, new() { Name = "Wallet" }).ClickAsync();
        await Expect(Page).ToHaveURLAsync(new Regex("/wallet$"));
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Expect(Page.Locator(".wallet-page > .beeday-hero")).ToBeVisibleAsync();

        var balance = Page.Locator(".wallet-summary__card--balance strong");
        await Expect(balance).ToHaveTextAsync("$0.00");
        var summaryCard = Page.Locator(".wallet-summary__card--balance");
        Assert.Equal("2px", await summaryCard.EvaluateAsync<string>("element => getComputedStyle(element).borderTopWidth"));
        Assert.Equal("none", await summaryCard.EvaluateAsync<string>("element => getComputedStyle(element).boxShadow"));

        var tagName = $"E2E Tag {Guid.NewGuid():N}"[..16];
        var tagDialog = Page.GetByRole(AriaRole.Dialog);
        await Page.GetByRole(AriaRole.Button, new() { Name = "New tag" }).ClickAsync();
        await Expect(tagDialog).ToBeVisibleAsync();
        await tagDialog.GetByLabel("Name").FillAsync(tagName);
        await tagDialog.GetByRole(AriaRole.Button, new() { Name = "Create" }).ClickAsync();
        await Expect(tagDialog).ToBeHiddenAsync();
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = $"Edit Tag: {tagName}" })).ToBeVisibleAsync();

        var transactionDialog = Page.GetByRole(AriaRole.Dialog);
        await Page.GetByRole(AriaRole.Button, new() { Name = "New transaction" }).ClickAsync();
        await Expect(transactionDialog).ToBeVisibleAsync();
        await transactionDialog.GetByLabel("Description").FillAsync("E2E income transaction");
        await transactionDialog.GetByLabel("Type").SelectOptionAsync(new SelectOptionValue { Label = "Income" });
        await transactionDialog.GetByLabel("Amount").FillAsync("150");
        await transactionDialog.GetByLabel("Tag").SelectOptionAsync(new SelectOptionValue { Label = tagName });
        await transactionDialog.GetByRole(AriaRole.Button, new() { Name = "Create" }).ClickAsync();
        await Expect(transactionDialog).ToBeHiddenAsync();

        await Expect(balance).ToHaveTextAsync("$150.00");
        await Expect(Page.Locator(".wallet-page")).ToHaveAttributeAsync("aria-busy", "false");

        var transactionCard = Page.Locator("[role='button'][aria-label^='Edit Transaction: E2E income transaction']");
        Assert.Equal("0", await transactionCard.GetAttributeAsync("tabindex"));
        await transactionCard.FocusAsync();
        await Page.Keyboard.PressAsync("Enter");
        await Expect(transactionDialog).ToBeVisibleAsync();
        await transactionDialog.GetByRole(AriaRole.Button, new() { Name = "Cancel" }).ClickAsync();
        await Expect(transactionDialog).ToBeHiddenAsync();

        await Page.SetViewportSizeAsync(390, 844);
        await Expect(summaryCard).ToBeVisibleAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "More Filters" }).ClickAsync();
        await Expect(Page.GetByLabel("From")).ToBeVisibleAsync();
        Assert.False(await Page.EvaluateAsync<bool>("() => document.documentElement.scrollWidth > document.documentElement.clientWidth"));
    }

    [Fact]
    public async Task MinimumTransaction_CreateEditAndDeleteInPortuguese_KeepsCircuitInteractive()
    {
        var email = $"e2e-wallet-pt-br-{Guid.NewGuid():N}@beeday.invalid";
        await Fixture.Factory.SeedUserAsync(email, Password, onboardingCompleted: true);

        await GotoAsync("/login");
        await Page.GetByLabel("Email").FillAsync(email);
        await Page.GetByLabel("Password").FillAsync(Password);
        await Page.GetByRole(AriaRole.Button, new() { Name = "Sign In" }).ClickAsync();
        await Expect(Page).ToHaveURLAsync(new Regex("/profile$"));

        await GotoAsync("/account");
        await Page.GetByLabel("Language").SelectOptionAsync(new SelectOptionValue { Label = "Portuguese" });
        await Page.GetByRole(AriaRole.Button, new() { Name = "Save Preferences" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Minha Conta" })).ToBeVisibleAsync();

        await GotoAsync("/wallet");
        await Expect(Page.Locator(".wallet-page > .beeday-hero")).ToBeVisibleAsync();

        var balance = Page.Locator(".wallet-summary__card--balance strong");
        await Expect(balance).ToHaveTextAsync(new Regex(@"^\$\s*0,00$"));

        var tagName = $"Tag {Guid.NewGuid():N}"[..16];
        var tagDialog = Page.GetByRole(AriaRole.Dialog);
        await Page.GetByRole(AriaRole.Button, new() { Name = "Nova tag" }).ClickAsync();
        await Expect(tagDialog).ToBeVisibleAsync();
        await tagDialog.GetByLabel("Nome").FillAsync(tagName);
        await tagDialog.GetByRole(AriaRole.Button, new() { Name = "Criar" }).ClickAsync();
        await Expect(tagDialog).ToBeHiddenAsync();

        const string description = "Receita minima E2E";
        var transactionDialog = Page.GetByRole(AriaRole.Dialog);
        await Page.GetByRole(AriaRole.Button, new() { Name = "Nova transação" }).ClickAsync();
        await Expect(transactionDialog).ToBeVisibleAsync();
        await transactionDialog.GetByLabel("Descrição").FillAsync(description);
        await transactionDialog.GetByLabel("Tipo").SelectOptionAsync(new SelectOptionValue { Label = "Receita" });
        await transactionDialog.GetByLabel("Valor").FillAsync("0.01");
        await transactionDialog.GetByLabel("Tag").SelectOptionAsync(new SelectOptionValue { Label = tagName });
        await transactionDialog.GetByRole(AriaRole.Button, new() { Name = "Criar" }).ClickAsync();
        await Expect(transactionDialog).ToBeHiddenAsync();

        await Expect(balance).ToHaveTextAsync(new Regex(@"^\$\s*0,01$"));
        await Expect(Page.Locator(".wallet-page")).ToHaveAttributeAsync("aria-busy", "false");

        var transactionCard = Page.Locator($"[role='button'][aria-label^='Editar transação: {description}']");
        await transactionCard.ClickAsync();
        await Expect(transactionDialog).ToBeVisibleAsync();
        await transactionDialog.GetByLabel("Valor").FillAsync("0.02");
        await transactionDialog.GetByRole(AriaRole.Button, new() { Name = "Salvar" }).ClickAsync();
        await Expect(transactionDialog).ToBeHiddenAsync();

        await Expect(balance).ToHaveTextAsync(new Regex(@"^\$\s*0,02$"));
        await Expect(Page.Locator(".wallet-page")).ToHaveAttributeAsync("aria-busy", "false");

        await transactionCard.ClickAsync();
        await Expect(transactionDialog).ToBeVisibleAsync();
        await transactionDialog.GetByRole(AriaRole.Button, new() { Name = "Excluir", Exact = true }).ClickAsync();
        await Expect(transactionDialog).ToBeHiddenAsync();

        var confirmation = Page.GetByRole(AriaRole.Alertdialog);
        await Expect(confirmation).ToBeVisibleAsync();
        await confirmation.GetByRole(AriaRole.Button, new() { Name = "Excluir transação" }).ClickAsync();
        await Expect(confirmation).ToBeHiddenAsync();

        await Expect(balance).ToHaveTextAsync(new Regex(@"^\$\s*0,00$"));
        await Expect(transactionCard).ToHaveCountAsync(0);
        await Expect(Page.Locator(".wallet-page")).ToHaveAttributeAsync("aria-busy", "false");
    }

    [Fact]
    public async Task Hero_IsRoughlyHalfTheHeightOfTheBrandGuidelinesHeroAndWorksWithoutAnIllustration()
    {
        // EPIC 27 Sprint 27.11 acceptance: "Hero não ocupa a altura completa de Brand guidelines;
        // referência ~50%." / "Wallet funciona sem imagem no slot direito."
        await Page.SetViewportSizeAsync(1280, 900);
        await LoginToWalletAsync();

        var walletHero = Page.Locator(".wallet-page > .beeday-hero");
        await Expect(walletHero).ToBeVisibleAsync();
        var walletHeroBox = await walletHero.BoundingBoxAsync();
        Assert.NotNull(walletHeroBox);

        await GotoAsync("/brand-guidelines");
        var brandHero = Page.Locator("header.beeday-hero");
        await Expect(brandHero).ToBeVisibleAsync();
        var brandHeroBox = await brandHero.BoundingBoxAsync();
        Assert.NotNull(brandHeroBox);

        var ratio = walletHeroBox!.Height / brandHeroBox!.Height;
        Assert.InRange(ratio, 0.3, 0.7);

        // No illustration was provided — the hero must still render its title/CTA correctly.
        await Expect(Page.Locator(".wallet-page > .beeday-hero .beeday-hero__illustration")).ToHaveCountAsync(0);
    }

    [Fact]
    public async Task SearchInput_ShowsExactlyOneFocusRing()
    {
        // EPIC 27 Sprint 27.11 acceptance: "Search apresenta apenas um focus ring."
        await LoginToWalletAsync();

        var search = Page.GetByLabel("Search", new() { Exact = true });
        await search.FocusAsync();
        await Expect(search).ToBeFocusedAsync();

        var outline = await search.EvaluateAsync<string>("element => getComputedStyle(element).outlineStyle");
        Assert.Equal("none", outline);

        var boxShadow = await search.EvaluateAsync<string>("element => getComputedStyle(element).boxShadow");
        Assert.NotEqual("none", boxShadow);
    }

    [Fact]
    public async Task OrganizationPanel_HeaderAndBodyShareTheSameRightEdgeWithoutAResidualStripe()
    {
        // EPIC 27 Sprint 27.11 acceptance: "Organization não possui faixa branca lateral inexplicada."
        await LoginToWalletAsync();

        var tagName = $"E2E Align {Guid.NewGuid():N}"[..16];
        var tagDialog = Page.GetByRole(AriaRole.Dialog);
        await Page.GetByRole(AriaRole.Button, new() { Name = "New tag" }).ClickAsync();
        await Expect(tagDialog).ToBeVisibleAsync();
        await tagDialog.GetByLabel("Name").FillAsync(tagName);
        await tagDialog.GetByRole(AriaRole.Button, new() { Name = "Create" }).ClickAsync();
        await Expect(tagDialog).ToBeHiddenAsync();

        var panel = Page.Locator(".wallet-tags-panel");
        var header = panel.Locator(".wallet-panel-header");
        var list = panel.Locator(".wallet-tag-list");
        await Expect(list).ToBeVisibleAsync();

        var panelBox = await panel.BoundingBoxAsync();
        var headerBox = await header.BoundingBoxAsync();
        var listBox = await list.BoundingBoxAsync();
        Assert.NotNull(panelBox);
        Assert.NotNull(headerBox);
        Assert.NotNull(listBox);

        var headerRightEdge = headerBox!.X + headerBox.Width;
        var listRightEdge = listBox!.X + listBox.Width;
        var panelRightEdge = panelBox!.X + panelBox.Width;

        // Header spans the panel's full width; the list's content is inset but by the *same*
        // amount on both sides, so its right edge sits a fixed, explained gap from the header's
        // (not an arbitrary leftover scrollbar-gutter stripe).
        Assert.InRange(Math.Abs(panelRightEdge - headerRightEdge), 0, 2);
        Assert.True(headerRightEdge - listRightEdge < panelBox.Width * 0.15,
            $"List right edge sits {headerRightEdge - listRightEdge}px short of the header's — too wide to be the intended inset alone.");
    }

    [Fact]
    public async Task TagColorPicker_OffersOnlyTheTenOfficialSwatchesWithAVisibleSelectionState()
    {
        // EPIC 27 Sprint 27.11 acceptance: "Nova tag só pode usar uma das 10 cores oficiais." /
        // "Seleção de cor é compreensível sem depender exclusivamente da cor."
        await LoginToWalletAsync();

        var tagDialog = Page.GetByRole(AriaRole.Dialog);
        await Page.GetByRole(AriaRole.Button, new() { Name = "New tag" }).ClickAsync();
        await Expect(tagDialog).ToBeVisibleAsync();

        await Expect(tagDialog.Locator("input[type='color']")).ToHaveCountAsync(0);
        var swatches = tagDialog.Locator(".wallet-color-swatch");
        await Expect(swatches).ToHaveCountAsync(10);

        var thirdSwatch = swatches.Nth(2);
        await thirdSwatch.ClickAsync();
        await Expect(thirdSwatch).ToHaveAttributeAsync("aria-checked", "true");
        await Expect(thirdSwatch.Locator(".beeday-icon")).ToBeVisibleAsync();

        var tagName = $"E2E Swatch {Guid.NewGuid():N}"[..16];
        await tagDialog.GetByLabel("Name").FillAsync(tagName);
        await tagDialog.GetByRole(AriaRole.Button, new() { Name = "Create" }).ClickAsync();
        await Expect(tagDialog).ToBeHiddenAsync();
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = $"Edit Tag: {tagName}" })).ToBeVisibleAsync();
    }

    private async Task LoginToWalletAsync()
    {
        var email = $"e2e-wallet-{Guid.NewGuid():N}@beeday.invalid";
        await Fixture.Factory.SeedUserAsync(email, Password, onboardingCompleted: true);

        await GotoAsync("/login");
        await Page.GetByLabel("Email").FillAsync(email);
        await Page.GetByLabel("Password").FillAsync(Password);
        await Page.GetByRole(AriaRole.Button, new() { Name = "Sign In" }).ClickAsync();
        await Expect(Page).ToHaveURLAsync(new Regex("/profile$"));
        await GotoAsync("/wallet");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Expect(Page.Locator(".wallet-page > .beeday-hero")).ToBeVisibleAsync();
    }
}
