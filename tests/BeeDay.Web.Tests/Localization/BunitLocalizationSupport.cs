using System.Globalization;
using Microsoft.Extensions.Localization;

namespace BeeDay.Web.Tests.Localization;

/// <summary>
/// Shared setup for bUnit tests rendering components that inject IStringLocalizer&lt;...&gt;
/// (AppFooter, PublicLayout, PublicHeader, Home, Login, ...). Every BunitContext needs
/// AddLogging()/AddLocalization() registered, same as production's builder.Services in
/// Program.cs; culture-sensitive tests additionally need both CultureInfo.CurrentCulture (number/
/// date formatting — e.g. WalletCurrencyFormatter) and CurrentUICulture (resx lookups) pinned, so
/// results don't depend on the machine's default locale. Both are pinned together because
/// RequestLocalizationMiddleware always sets them to the same resolved culture in production
/// (BeeDayCultures never separates "c=" from "uic=" in the culture cookie) — pinning only one, as
/// an earlier version of this helper did, let CurrentCulture silently fall through to the
/// machine's ambient default and produced flaky currency/date-formatting assertions.
/// </summary>
internal static class BunitLocalizationSupport
{
    public static BunitContext WithLocalization(this BunitContext context)
    {
        context.Services.AddLogging();
        context.Services.AddLocalization();
        return context;
    }

    /// <summary>
    /// Pins the culture only for the duration of <paramref name="render"/> — the culture is already
    /// restored to the ambient default by the time this method returns the rendered component.
    /// </summary>
    /// <remarks>
    /// Common mistake (caused six real CI failures — the dev machine's ambient pt-BR masked it
    /// locally, since a re-render outside the scope still happened to land in the right culture by
    /// coincidence): wrapping only the initial <c>Render&lt;T&gt;(...)</c> call here and then
    /// interacting with the returned component (<c>.Click()</c>, <c>InvokeAsync(...)</c>,
    /// <c>.Change(...)</c>) afterward. Any interaction that causes Blazor to re-execute
    /// <c>BuildRenderTree</c> — opening a conditionally-rendered dialog/menu, a toast arriving via a
    /// service event — re-evaluates every <c>@Localizer[...]</c> expression in that component,
    /// including ones that already rendered correctly the first time, against whatever culture is
    /// ambient at that later moment. If the test needs to click, wait, or otherwise trigger a
    /// re-render before asserting culture-sensitive text, do that (and the assertion) inside the
    /// synchronous <see cref="WithUiCulture(string, Action)"/> overload instead, or
    /// <see cref="WithUiCultureAsync"/> for anything that must stay pinned across an <c>await</c>.
    /// </remarks>
    public static T WithUiCulture<T>(string culture, Func<T> render)
    {
        var (restoreCulture, restoreUiCulture) = (CultureInfo.CurrentCulture, CultureInfo.CurrentUICulture);
        try
        {
            (CultureInfo.CurrentCulture, CultureInfo.CurrentUICulture) = (CultureInfo.GetCultureInfo(culture), CultureInfo.GetCultureInfo(culture));
            return render();
        }
        finally
        {
            (CultureInfo.CurrentCulture, CultureInfo.CurrentUICulture) = (restoreCulture, restoreUiCulture);
        }
    }

    public static void WithUiCulture(string culture, Action action)
    {
        var (restoreCulture, restoreUiCulture) = (CultureInfo.CurrentCulture, CultureInfo.CurrentUICulture);
        try
        {
            (CultureInfo.CurrentCulture, CultureInfo.CurrentUICulture) = (CultureInfo.GetCultureInfo(culture), CultureInfo.GetCultureInfo(culture));
            action();
        }
        finally
        {
            (CultureInfo.CurrentCulture, CultureInfo.CurrentUICulture) = (restoreCulture, restoreUiCulture);
        }
    }

    /// <summary>
    /// Async counterpart of <see cref="WithUiCulture{T}"/> — needed whenever the pinned culture
    /// must still be in effect after an `await` (e.g. a bUnit click that triggers a re-render, or
    /// a non-UI async operation like DashboardState.InitializeAsync producing a toast message),
    /// since the synchronous overload restores the culture as soon as its delegate returns.
    /// </summary>
    public static async Task WithUiCultureAsync(string culture, Func<Task> action)
    {
        var (restoreCulture, restoreUiCulture) = (CultureInfo.CurrentCulture, CultureInfo.CurrentUICulture);
        try
        {
            (CultureInfo.CurrentCulture, CultureInfo.CurrentUICulture) = (CultureInfo.GetCultureInfo(culture), CultureInfo.GetCultureInfo(culture));
            await action();
        }
        finally
        {
            (CultureInfo.CurrentCulture, CultureInfo.CurrentUICulture) = (restoreCulture, restoreUiCulture);
        }
    }
}

/// <summary>
/// Overrides a single resource key with an arbitrary (possibly adversarial) value, falling back
/// to the key name itself for everything else — used by escaping tests that need to render a
/// page with one specific resource forced to a value containing quotes/HTML/JS-special
/// characters, without needing a real culture or resx lookup for the rest of the page.
/// </summary>
internal sealed class SingleKeyOverrideLocalizer<T>(string overriddenKey, string overriddenValue) : IStringLocalizer<T>
{
    public LocalizedString this[string name] =>
        new(name, name == overriddenKey ? overriddenValue : name);

    public LocalizedString this[string name, params object[] arguments] => this[name];

    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => [];
}
