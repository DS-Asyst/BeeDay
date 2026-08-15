using System.Globalization;
using Microsoft.Extensions.Localization;

namespace BeeDay.Web.Tests.Localization;

/// <summary>
/// Shared setup for bUnit tests rendering components that inject IStringLocalizer&lt;...&gt;
/// (AppFooter, PublicLayout, PublicHeader, Home, Login, ...). Every BunitContext needs
/// AddLogging()/AddLocalization() registered, same as production's builder.Services in
/// Program.cs; UI-culture-sensitive tests additionally need CultureInfo.CurrentUICulture pinned
/// so results don't depend on the machine's default locale.
/// </summary>
internal static class BunitLocalizationSupport
{
    public static BunitContext WithLocalization(this BunitContext context)
    {
        context.Services.AddLogging();
        context.Services.AddLocalization();
        return context;
    }

    public static T WithUiCulture<T>(string culture, Func<T> render)
    {
        var restore = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(culture);
            return render();
        }
        finally
        {
            CultureInfo.CurrentUICulture = restore;
        }
    }

    public static void WithUiCulture(string culture, Action action)
    {
        var restore = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(culture);
            action();
        }
        finally
        {
            CultureInfo.CurrentUICulture = restore;
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
        var restore = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(culture);
            await action();
        }
        finally
        {
            CultureInfo.CurrentUICulture = restore;
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
