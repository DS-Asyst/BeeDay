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
        await Expect(Page).ToHaveURLAsync(new Regex("/daily$"));
        // See AccountLifecycleTests.LoginAsync's remarks: a redirect-triggered navigation
        // establishes its own SignalR circuit that GotoAsync's own wait cannot cover.
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await Page.GetByRole(AriaRole.Link, new() { Name = "Wallet" }).ClickAsync();
        await Expect(Page).ToHaveURLAsync(new Regex("/wallet$"));
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

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
        Assert.False(await Page.EvaluateAsync<bool>("() => document.documentElement.scrollWidth > document.documentElement.clientWidth"));
    }
}
