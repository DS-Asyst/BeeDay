namespace BeeDay.Web.Tests.Components.Visual;

/// <summary>
/// Guards the global visual foundation established by Epic 21 Sprint 21.4. These are source-level
/// contracts because bUnit does not compute linked stylesheets or download web fonts.
/// </summary>
public sealed class VisualFoundationTests
{
    private static readonly string RepoRoot = ResolveRepoRoot();

    [Fact]
    public void BrandFamilyUsesTheOfficialPrimaryPaletteWithoutASecondBrandColor()
    {
        var css = ReadWebFile("wwwroot", "css", "variables.css");

        Assert.Contains("--beeday-color-brand-primary: #5247f9;", css, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("--beeday-color-brand-primary-hover: #3f33f1;", css, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("--beeday-color-brand-primary-active: #1c0ef2;", css, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("--beeday-color-brand-primary-light: #827afc;", css, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("--beeday-color-brand-primary-soft: #f8f7ff;", css, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("--beeday-color-brand-yellow: var(--beeday-color-reward);", css, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("--beeday-color-brand-yellow-hover: var(--beeday-color-reward-hover);", css, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("--beeday-color-brand-yellow-foreground: var(--beeday-color-reward-foreground);", css, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("#1023c8", css, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("#1e33ed", css, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("#0c1b99", css, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("#3044d6", css, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("--beeday-game-yellow", css, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("--lingo-", css, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("--epic21-", css, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void PublicHomeAcquisitionCtaUsesTheSharedBrandPrimaryTokenWithoutAParallelColor()
    {
        var variables = ReadWebFile("wwwroot", "css", "variables.css");
        var home = ReadWebFile("Components", "Features", "Home", "Pages", "Home.razor.css");

        Assert.DoesNotContain("public-home-cta", variables, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("public-home-cta", home, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("--beeday-button-background", home, StringComparison.Ordinal);
        Assert.DoesNotContain("#14adff", home, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("#2cbaff", home, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("#0079b9", home, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void SharedBrandUsesOfficialPrimaryByDefaultAndRealWhiteOnDarkSurfaces()
    {
        // EPIC 27 Sprint 27.1: the inverse lockup used on brand/dark surfaces (Footer, brand-surface
        // heroes) now renders real white instead of repeating Brand Primary as a historical no-op.
        var brand = ReadWebFile("Components", "DesignSystem", "Text", "BeeDayBrand.razor.css");

        Assert.Contains(".beeday-brand__bee,", brand, StringComparison.Ordinal);
        Assert.Contains(".beeday-brand__day { color: var(--beeday-color-brand-primary); }", brand, StringComparison.Ordinal);
        Assert.Contains(".beeday-brand--inverse .beeday-brand__bee,", brand, StringComparison.Ordinal);
        Assert.Contains(".beeday-brand--inverse .beeday-brand__day { color: var(--beeday-color-text-inverse); }", brand, StringComparison.Ordinal);
        Assert.Contains(".beeday-brand__icon {", brand, StringComparison.Ordinal);
        Assert.DoesNotContain("--beeday-color-brand-yellow", brand, StringComparison.Ordinal);
    }

    [Fact]
    public void AuthenticatedSidebarCentersItsBrandAndGivesAccountATextSafeAccentOnlyWhenInactive()
    {
        // EPIC 27 Sprint 27.9: the sidebar's brand lockup is centered and sized for the shell chrome
        // (was left-aligned at the bare 1.75rem component default before this sprint); Account uses
        // the text-safe COR3 accent (03_DESIGN_DECISIONS.md §4) but only in its resting state, so it
        // never borrows the active-route pill it isn't entitled to when it isn't the current page.
        var desktopSidebar = ReadWebFile("Components", "Layout", "DesktopSidebar.razor.css");
        var navigationItems = ReadWebFile("Components", "Layout", "NavigationItems.razor.css");

        Assert.Contains("justify-content: center;", desktopSidebar, StringComparison.Ordinal);
        Assert.Contains("--beeday-brand-height: 2.25rem;", desktopSidebar, StringComparison.Ordinal);

        Assert.Contains(".navigation-items__account ::deep .navigation-item:not(.is-active) { color: var(--beeday-color-accent-secondary-on-light); }", navigationItems, StringComparison.Ordinal);
        Assert.DoesNotContain(".navigation-items__account ::deep .navigation-item.is-active", navigationItems, StringComparison.Ordinal);
    }

    [Fact]
    public void ActivityCardCheckboxIsATrueEmptyBoxAtRestAndNeverPreviewsTheCheckBeforeCompletion()
    {
        // EPIC 27 Sprint 27.10 made pending/default a real empty box (no visible check, but a
        // visible border) — that part still holds. Sprint 29.3 removed the hover/focus/active
        // "preview" it also introduced (glyph fading to opacity .62 on hover alone): a task/to-do
        // hovered before being completed visually suggested a check mark, which read as a false
        // completed affordance. The glyph now appears only once the item is actually completed;
        // hover/focus/active may still change surface/border/scale for affordance, just not the
        // glyph's own opacity.
        var cards = ReadWebFile("wwwroot", "css", "cards.css");

        Assert.Contains("border: 2px solid currentColor;", cards, StringComparison.Ordinal);
        Assert.Contains(".activity-card__checkbox-glyph { opacity: 0; }", cards, StringComparison.Ordinal);
        Assert.Contains(".activity-card--completed .activity-card__checkbox-glyph { opacity: 1; }", cards, StringComparison.Ordinal);
        Assert.Contains(".activity-card__checkbox:focus-visible {", cards, StringComparison.Ordinal);
        Assert.Contains("transform: scale(.92);", cards, StringComparison.Ordinal);
        Assert.DoesNotContain(":hover .activity-card__checkbox-glyph", cards, StringComparison.Ordinal);
        Assert.DoesNotContain(":focus-visible .activity-card__checkbox-glyph", cards, StringComparison.Ordinal);
        Assert.DoesNotContain(":active .activity-card__checkbox-glyph", cards, StringComparison.Ordinal);
        Assert.DoesNotContain("opacity: .62", cards, StringComparison.Ordinal);
        Assert.DoesNotContain("opacity:.62", cards, StringComparison.Ordinal);
    }

    [Fact]
    public void ActivityCardProjectBadgeReusesTheExistingBeeIllustrationOnTheProjectAccent()
    {
        // EPIC 27 Sprint 27.10: no new bee character was generated — project-bee.png is a cropped
        // presentation of the same source art as assets/brand/bee-color-neutral.png (renamed from
        // assets/home/how-beeday-works-bee.png in Sprint 29.1; see 06_ASSETS_AND_OPEN_ITEMS.md).
        var cards = ReadWebFile("wwwroot", "css", "cards.css");
        var component = ReadWebFile("Components", "Features", "Dashboard", "Components", "ActivityCard.razor");

        Assert.Contains("src=\"/assets/dashboard/project-bee.png\"", component, StringComparison.Ordinal);
        Assert.DoesNotContain("BeeDayIconName.Project", component, StringComparison.Ordinal);
        Assert.Contains(".activity-card--project .activity-card__project-status {", cards, StringComparison.Ordinal);
        Assert.Contains("background: var(--beeday-color-project);", cards, StringComparison.Ordinal);
        Assert.True(File.Exists(Path.Combine(RepoRoot, "src", "BeeDay.Web", "wwwroot", "assets", "dashboard", "project-bee.png")));
    }

    [Fact]
    public void MobileHeaderAndDrawerBrandLinksAreReachableThroughDeepSelectors()
    {
        // EPIC 27 Sprint 27.12 (epic hardening): DesktopSidebar's brand link had the exact same
        // bug when it was fixed in Sprint 27.9 — NavLink wraps a child component (BeeDayBrand),
        // so its rendered <a> never receives the file's own CSS-isolation scope attribute, and an
        // un-::deep'd rule targeting it is a silent no-op. That sprint's own commit flagged
        // MobileSidebar as a known, deferred instance of the same bug; MobileHeader had the
        // identical bug too, undiagnosed until this hardening pass. Both rendered their wordmark
        // with the browser's default underline instead of beeday-brand's own styling.
        var mobileHeader = ReadWebFile("Components", "Layout", "MobileHeader.razor.css");
        var mobileSidebar = ReadWebFile("Components", "Layout", "MobileSidebar.razor.css");

        Assert.Contains(".mobile-header ::deep .mobile-header__brand", mobileHeader, StringComparison.Ordinal);
        Assert.Contains(".mobile-nav-drawer__header ::deep .mobile-nav-drawer__brand", mobileSidebar, StringComparison.Ordinal);
    }

    [Fact]
    public void Epic27PaletteFoundationsAreCentralizedAndPairedWithAForeground()
    {
        var variables = ReadWebFile("wwwroot", "css", "variables.css");
        var utilities = ReadWebFile("wwwroot", "css", "utilities.css");
        var designSystem = ReadWebFile("wwwroot", "css", "design-system.css");

        Assert.Contains("--beeday-palette-cor0: var(--beeday-color-brand-primary);", variables, StringComparison.Ordinal);
        Assert.Contains("--beeday-palette-cor1: #ce82ff;", variables, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("--beeday-palette-cor2: #58cc02;", variables, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("--beeday-palette-cor3: #1cb0f6;", variables, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("--beeday-palette-cor4: #ffb100;", variables, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("--beeday-palette-cor5: #ff7878;", variables, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("--beeday-palette-cor6: #ffffff;", variables, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("--beeday-palette-cor7: #ececed;", variables, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("--beeday-palette-cor8: #100f3e;", variables, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("--beeday-palette-cor9: #defff7;", variables, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("--beeday-color-accent-secondary: var(--beeday-palette-cor3);", variables, StringComparison.Ordinal);
        Assert.Contains("--beeday-hero-surface-default: var(--beeday-palette-cor0);", variables, StringComparison.Ordinal);
        // The button text uses the text-safe accent variant, not raw COR3: raw COR3 on white measures
        // ~2.44:1 (fails WCAG AA's 4.5:1 normal-text minimum, confirmed by the repo's axe-core E2E scan).
        Assert.Contains("--beeday-color-accent-secondary-on-light: #0b72a6;", variables, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("--beeday-color-button-important-white-fg: var(--beeday-color-accent-secondary-on-light);", variables, StringComparison.Ordinal);

        Assert.Contains(".beeday-link {", utilities, StringComparison.Ordinal);
        Assert.Contains("text-decoration: none;", utilities, StringComparison.Ordinal);
        for (var i = 0; i <= 9; i++)
        {
            Assert.Contains($".beeday-surface-cor{i} {{ background: var(--beeday-palette-cor{i}); color: var(--beeday-palette-cor{i}-foreground); }}", utilities, StringComparison.Ordinal);
        }

        Assert.Contains(".beeday-button--important-white {", designSystem, StringComparison.Ordinal);
        // .beeday-button is applied to real <button>s (never underlined) and <a>s used as buttons
        // (PublicAuthActions, Home's hero CTAs, the institutional hero's PrimaryAction). Anchors
        // default to underlined text; the base rule must reset it once instead of relying on every
        // consumer to add its own local override (the institutional hero's CTA rendered underlined
        // in Sprint 27.6 because no such override existed for that new consumer).
        var baseButtonRuleHasNoUnderline = System.Text.RegularExpressions.Regex.IsMatch(
            designSystem, @"\.beeday-button\s*\{[^}]*text-decoration:\s*none", System.Text.RegularExpressions.RegexOptions.Singleline);
        Assert.True(baseButtonRuleHasNoUnderline, "The base .beeday-button rule must reset text-decoration for anchor consumers.");
    }

    [Fact]
    public void HeroSurfacePairingsAreDefinedInsideTheIsolatedStylesheetNotThePlainUtilityClass()
    {
        // Blazor CSS isolation adds a scope attribute to .beeday-hero, which outranks a plain
        // .beeday-surface-corN utility class regardless of load order (discovered in Sprint 27.3 —
        // the institutional hero silently rendered white instead of its chosen COR surface until
        // fixed). The pairing must be re-declared as a compound selector inside BeeDayHero.razor.css.
        var hero = ReadWebFile("Components", "DesignSystem", "Layout", "BeeDayHero.razor.css");

        Assert.Contains("background: var(--beeday-hero-surface-bg, var(--beeday-color-surface));", hero, StringComparison.Ordinal);
        for (var i = 0; i <= 9; i++)
        {
            Assert.Contains($".beeday-hero.beeday-surface-cor{i} {{ --beeday-hero-surface-bg: var(--beeday-palette-cor{i}); --beeday-hero-surface-fg: var(--beeday-palette-cor{i}-foreground); }}", hero, StringComparison.Ordinal);
        }

        // BeeDayBrand's own fixed default/inverse colors do not reliably contrast against every
        // COR0-COR9 surface (brand-primary purple text failed WCAG color-contrast against COR3/COR4
        // in a real axe-core E2E run); the hero's brand-context slot must force the lockup text to
        // inherit the surface's own paired foreground instead.
        Assert.Contains(".beeday-hero__brand-context ::deep .beeday-brand__bee,", hero, StringComparison.Ordinal);
        Assert.Contains(".beeday-hero__brand-context ::deep .beeday-brand__day,", hero, StringComparison.Ordinal);
        // Sprint 29.1: InstitutionalPageShell passes OnDarkSurface="true" so BeeDayBrand selects its
        // non-white-background icon, which also applies .beeday-brand--inverse (fixed white text) —
        // a plain 2-class-selector rule only outranks BeeDayBrand's *default* text rule (0,2,0), not
        // --inverse's own scoped rule (0,4,0); this more specific pair (0,5,0) is required to also
        // outrank --inverse regardless of the two scoped files' bundling order.
        Assert.Contains(".beeday-hero__brand-context ::deep .beeday-brand.beeday-brand--inverse .beeday-brand__bee,", hero, StringComparison.Ordinal);
        Assert.Contains(".beeday-hero__brand-context ::deep .beeday-brand.beeday-brand--inverse .beeday-brand__day { color: inherit; }", hero, StringComparison.Ordinal);
    }

    [Fact]
    public void FooterSocialListOverridesTheGroupListDisplayWithSufficientSpecificity()
    {
        // Plain CSS specificity, no Blazor isolation involved: the base ".app-footer__group ul {
        // display: grid; }" rule (class+type selector) silently beat a same-file ".app-footer__
        // social-list { display: flex; }" override (class-only selector, same file, later source
        // order — order does not matter once specificity differs) — social icons rendered stacked
        // vertically instead of in a row until this was corrected (Sprint 27.4).
        var footer = ReadWebFile("Components", "Layout", "AppFooter.razor.css");
        Assert.Contains(".app-footer__group ul.app-footer__social-list { display: flex;", footer, StringComparison.Ordinal);
    }

    [Fact]
    public void FooterTextDoesNotDimAlreadyPairedWhiteBelowWcagAaAndSocialPlaceholdersHaveAValidAriaRole()
    {
        // White text opacity-dimmed to .82/.78 over the COR0 background measured ~4.42:1/~4.15:1,
        // under WCAG AA's 4.5:1 minimum — caught live by the repo's axe-core E2E scan on every
        // public route (the footer is global). Hierarchy must come from size/weight, not opacity,
        // for any footer text that is always rendered (not just a :hover/:focus transient state).
        var footerCss = ReadWebFile("Components", "Layout", "AppFooter.razor.css");
        Assert.Contains(".app-footer__identity p { margin: 0; color: inherit; }", footerCss, StringComparison.Ordinal);
        var copyrightBlockHasNoOpacity = !System.Text.RegularExpressions.Regex.IsMatch(
            footerCss, @"\.app-footer__copyright\s*\{[^}]*opacity", System.Text.RegularExpressions.RegexOptions.Singleline);
        Assert.True(copyrightBlockHasNoOpacity, "app-footer__copyright must not dim its text via opacity.");

        // aria-label alone is prohibited on a bare <span> (role "generic") per the ARIA spec — axe
        // flagged this as aria-prohibited-attr. role="img" makes it a valid target for aria-label.
        var footerRazor = ReadWebFile("Components", "Layout", "AppFooter.razor");
        Assert.Contains("class=\"app-footer__social-unavailable\" role=\"img\" aria-label=", footerRazor, StringComparison.Ordinal);
    }

    [Fact]
    public void RewardAndButtonAliasesPreserveSemanticOwnership()
    {
        var variables = ReadWebFile("wwwroot", "css", "variables.css");
        var progress = ReadWebFile("Components", "DesignSystem", "Progress", "BeeDayProgressBar.razor.css");
        var experience = ReadWebFile("Components", "Features", "Experience", "Components", "ExperienceBar.razor.css");

        Assert.Contains("--beeday-color-reward: #ffd326;", variables, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("--beeday-color-reward-active: #cda600;", variables, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("--beeday-color-reward-foreground: var(--beeday-color-text-primary);", variables, StringComparison.Ordinal);
        Assert.Contains("--beeday-color-button-success-fg: var(--beeday-color-text-inverse);", variables, StringComparison.Ordinal);
        Assert.Contains("--beeday-color-button-danger-fg: var(--beeday-color-text-inverse);", variables, StringComparison.Ordinal);
        Assert.Contains("--beeday-color-button-confirmation-cancel-bg: var(--beeday-color-surface);", variables, StringComparison.Ordinal);
        Assert.Contains("var(--beeday-color-reward)", progress, StringComparison.Ordinal);
        Assert.Contains("var(--beeday-color-reward)", experience, StringComparison.Ordinal);
        Assert.DoesNotContain("var(--beeday-color-brand-yellow", progress, StringComparison.Ordinal);
        Assert.DoesNotContain("var(--beeday-color-brand-yellow", experience, StringComparison.Ordinal);
    }

    [Fact]
    public void NunitoOwnsProductTypographyAndCoinyOwnsBrandDisplay()
    {
        var typography = ReadWebFile("wwwroot", "css", "typography.css");
        var app = ReadWebFile("Components", "App.razor");
        var sourceFiles = Directory.EnumerateFiles(
            Path.Combine(RepoRoot, "src", "BeeDay.Web"),
            "*",
            SearchOption.AllDirectories)
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                && !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .Where(path => Path.GetExtension(path) is ".css" or ".razor" or ".cs")
            .Select(File.ReadAllText);

        Assert.Contains("--beeday-font-family: var(--beeday-font-body);", typography, StringComparison.Ordinal);
        Assert.Contains("--beeday-font-display: \"Coiny\", \"Nunito\", \"Segoe UI\", sans-serif;", typography, StringComparison.Ordinal);
        Assert.Contains("--beeday-type-brand-display:", typography, StringComparison.Ordinal);
        Assert.Contains("family=Coiny&family=Nunito:wght@400;500;600;700;800;900", app, StringComparison.Ordinal);
        Assert.DoesNotContain("Jersey", app, StringComparison.OrdinalIgnoreCase);
        Assert.All(sourceFiles, content =>
        {
            Assert.DoesNotContain("Jersey 25", content, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("--beeday-font-ui", content, StringComparison.Ordinal);
        });
    }

    [Fact]
    public void SharedShapeLanguageUsesCanonicalSpacingBordersAndDepthWithoutFlatteningFeatures()
    {
        var variables = ReadWebFile("wwwroot", "css", "variables.css");
        var activity = ReadWebFile("wwwroot", "css", "activity-design-system.css");
        var polish = ReadWebFile("wwwroot", "css", "polish.css");
        var designSystem = ReadWebFile("wwwroot", "css", "design-system.css");
        var forms = ReadWebFile("wwwroot", "css", "forms.css");
        var feedback = ReadWebFile("wwwroot", "css", "feedback.css");
        var editorModal = ReadWebFile("wwwroot", "css", "editor-modal.css");
        var projectWorkspace = ReadWebFile("Components", "Features", "Projects", "Components", "ProjectWorkspace.razor.css");
        var beeDayFeedbackModal = ReadWebFile("Components", "Features", "Experience", "Feedback", "BeeDayFeedbackModal.razor.css");
        var mobileSidebar = ReadWebFile("Components", "Layout", "MobileSidebar.razor.css");

        Assert.Contains("--beeday-border-width-subtle: 1px;", variables, StringComparison.Ordinal);
        Assert.Contains("--beeday-border-width: 2px;", variables, StringComparison.Ordinal);
        Assert.Contains("--beeday-depth-sm: 2px;", variables, StringComparison.Ordinal);
        Assert.Contains("--beeday-depth-md: 4px;", variables, StringComparison.Ordinal);
        Assert.Contains("--beeday-depth-lg: 8px;", variables, StringComparison.Ordinal);
        Assert.Contains("--beeday-shadow-sm:", variables, StringComparison.Ordinal);
        Assert.Contains("--beeday-shadow-lg:", variables, StringComparison.Ordinal);

        Assert.Contains("--beeday-grid: var(--beeday-spacing-sm);", polish, StringComparison.Ordinal);
        Assert.Contains("--activity-space-xs: var(--beeday-spacing-xs);", activity, StringComparison.Ordinal);
        Assert.Contains("--activity-space-sm: var(--beeday-spacing-sm);", activity, StringComparison.Ordinal);
        Assert.Contains("--activity-space-md: var(--beeday-spacing-smd);", activity, StringComparison.Ordinal);
        Assert.Contains("--activity-space-lg: var(--beeday-spacing-md);", activity, StringComparison.Ordinal);
        Assert.Contains("--activity-radius-md: .4rem;", activity, StringComparison.Ordinal);

        Assert.Contains("padding: var(--beeday-spacing-sm) var(--beeday-spacing-md);", designSystem, StringComparison.Ordinal);
        Assert.Contains("border-bottom-width: var(--beeday-depth-md);", designSystem, StringComparison.Ordinal);
        Assert.Contains("transform: translateY(var(--beeday-depth-md));", designSystem, StringComparison.Ordinal);
        Assert.Contains("border-radius: var(--beeday-radius-lg);", designSystem, StringComparison.Ordinal);
        Assert.Contains("box-shadow: none;", designSystem, StringComparison.Ordinal);
        Assert.Contains("border: var(--beeday-border-width) solid var(--beeday-color-border);", forms, StringComparison.Ordinal);
        Assert.Contains("border: var(--beeday-border-width-subtle) solid var(--beeday-color-border);", feedback, StringComparison.Ordinal);
        Assert.Contains("border: var(--beeday-border-width-subtle) solid var(--beeday-color-border);", editorModal, StringComparison.Ordinal);

        Assert.Contains("backdrop-filter: blur(3px);", projectWorkspace, StringComparison.Ordinal);
        Assert.Contains("background: var(--beeday-color-overlay);", projectWorkspace, StringComparison.Ordinal);
        Assert.Contains("z-index: var(--beeday-z-modal);", projectWorkspace, StringComparison.Ordinal);
        Assert.DoesNotMatch("#[0-9a-fA-F]{3,8}", projectWorkspace);
        Assert.DoesNotContain("--beeday-radius-control", variables, StringComparison.Ordinal);
        Assert.DoesNotContain("--beeday-shadow-activity", variables, StringComparison.Ordinal);

        // Sprint 29.3: every full-viewport modal/drawer scrim now shares the same canonical
        // --beeday-color-overlay token instead of each hardcoding its own purple/violet literal
        // (four different rgb() values existed before this Sprint) — a light/neutral/translucent
        // backdrop is the approved contract; it must not compete visually with the modal itself.
        Assert.Contains("background: var(--beeday-color-overlay);", editorModal, StringComparison.Ordinal);
        Assert.Contains("background: var(--beeday-color-overlay);", feedback, StringComparison.Ordinal);
        Assert.Contains("background: var(--beeday-color-overlay);", beeDayFeedbackModal, StringComparison.Ordinal);
        Assert.Contains("background: var(--beeday-color-overlay);", mobileSidebar, StringComparison.Ordinal);
        Assert.DoesNotContain("rgb(35 25 45", editorModal, StringComparison.Ordinal);
        Assert.DoesNotContain("rgb(47 27 72", feedback, StringComparison.Ordinal);
        Assert.DoesNotContain("rgb(35 18 56", beeDayFeedbackModal, StringComparison.Ordinal);
        Assert.DoesNotContain("rgb(35 25 45", mobileSidebar, StringComparison.Ordinal);
    }

    [Fact]
    public void GlobalBackgroundIsASolidFoundationSurface()
    {
        var css = ReadWebFile("wwwroot", "app.css");

        Assert.Contains("body { background: var(--beeday-color-background); }", css, StringComparison.Ordinal);
        Assert.DoesNotContain("repeating-linear-gradient", css, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void NeutralNavigationConsumesSharedMotionAndBrandFocusFoundations()
    {
        var navigationItem = ReadWebFile("Components", "Layout", "NavigationItem.razor.css");
        var mobileSidebar = ReadWebFile("Components", "Layout", "MobileSidebar.razor.css");

        Assert.Contains("var(--beeday-transition-normal)", navigationItem, StringComparison.Ordinal);
        Assert.Contains("var(--beeday-color-brand-primary)", navigationItem, StringComparison.Ordinal);
        Assert.Contains("var(--beeday-transition-emphasized)", mobileSidebar, StringComparison.Ordinal);
        Assert.Contains("@media (prefers-reduced-motion: reduce)", mobileSidebar, StringComparison.Ordinal);
    }

    [Fact]
    public void SharedMotionReducedMotionAndLayerContractsRemainCoherent()
    {
        var variables = ReadWebFile("wwwroot", "css", "variables.css");
        var feedback = ReadWebFile("wwwroot", "css", "feedback.css");
        var editorModal = ReadWebFile("wwwroot", "css", "editor-modal.css");
        var experienceFeedback = ReadWebFile("Components", "Features", "Experience", "Feedback", "BeeDayFeedbackModal.razor.css");
        var reconnect = ReadWebFile("Components", "Layout", "ReconnectModal.razor.css");
        var mobileSidebar = ReadWebFile("Components", "Layout", "MobileSidebar.razor.css");
        var home = ReadWebFile("Components", "Features", "Home", "Pages", "Home.razor.css");

        Assert.Contains("--beeday-duration-fast: 120ms;", variables, StringComparison.Ordinal);
        Assert.Contains("--beeday-duration-normal: 180ms;", variables, StringComparison.Ordinal);
        Assert.Contains("--beeday-duration-slow: 260ms;", variables, StringComparison.Ordinal);
        Assert.Contains("--beeday-easing-standard:", variables, StringComparison.Ordinal);
        Assert.Contains("--beeday-easing-emphasized:", variables, StringComparison.Ordinal);

        Assert.Contains("z-index: var(--beeday-z-confirmation);", feedback, StringComparison.Ordinal);
        Assert.Contains("z-index: var(--beeday-z-modal-raised);", editorModal, StringComparison.Ordinal);
        Assert.Contains("z-index: var(--beeday-z-drawer-backdrop);", mobileSidebar, StringComparison.Ordinal);
        Assert.Contains("z-index: var(--beeday-z-drawer);", mobileSidebar, StringComparison.Ordinal);
        Assert.Contains("--beeday-z-toast: 1700;", variables, StringComparison.Ordinal);

        Assert.DoesNotContain("var(--beeday-transition-normal)-out", feedback, StringComparison.Ordinal);
        Assert.DoesNotContain("var(--beeday-transition-normal) ease-out", experienceFeedback, StringComparison.Ordinal);
        Assert.Contains("animation: delete-confirmation-enter var(--beeday-duration-normal) var(--beeday-easing-emphasized) both;", feedback, StringComparison.Ordinal);
        Assert.Contains("animation: beeday-feedback-enter var(--beeday-duration-normal) var(--beeday-easing-emphasized) both;", experienceFeedback, StringComparison.Ordinal);

        Assert.Contains(".beeday-loading-overlay {", feedback, StringComparison.Ordinal);
        Assert.Contains("opacity: 1;", feedback, StringComparison.Ordinal);
        Assert.Contains("#components-reconnect-modal[open]", reconnect, StringComparison.Ordinal);
        Assert.Contains("animation: none;", reconnect, StringComparison.Ordinal);
        Assert.Contains("@media (prefers-reduced-motion: reduce)", home, StringComparison.Ordinal);
        Assert.Contains("background: #d5eefd;", home, StringComparison.Ordinal);
    }

    // EXP32-F010 (Sprint 32.15): 4 of the 8 stylesheets identified in Sprint 25.6 as depending only
    // on the global .01ms safety net (docs/ux/02-accessibility.md §6) were Daily-journey owned -
    // Habit, ProjectWorkspace, and the shared activity/habit card foundation - plus a 4th gap this
    // same Sprint 32.13 introduced (the skip link's own transition). Each now has a local
    // transition:none fallback alongside the transition it neutralizes.
    [Fact]
    public void DailyJourneyStylesheetsHaveLocalReducedMotionFallbacks()
    {
        var cards = ReadWebFile("wwwroot", "css", "cards.css");
        var projectWorkspace = ReadWebFile("Components", "Features", "Projects", "Components", "ProjectWorkspace.razor.css");
        var habitEditorModal = ReadWebFile("Components", "Features", "Habits", "Components", "HabitEditorModal.razor.css");
        var utilities = ReadWebFile("wwwroot", "css", "utilities.css");

        Assert.Contains("transition:transform .12s ease,background-color .12s ease,color .12s ease", cards, StringComparison.Ordinal);
        Assert.Contains("@media (prefers-reduced-motion: reduce)", cards, StringComparison.Ordinal);

        Assert.Contains("transition: color .12s ease, border-color .12s ease, background-color .12s ease;", projectWorkspace, StringComparison.Ordinal);
        Assert.Contains("@media (prefers-reduced-motion: reduce)", projectWorkspace, StringComparison.Ordinal);

        Assert.Contains("transition: transform 140ms ease, background-color 140ms ease;", habitEditorModal, StringComparison.Ordinal);
        Assert.Contains("@media (prefers-reduced-motion: reduce)", habitEditorModal, StringComparison.Ordinal);

        Assert.Contains(".skip-to-content-link", utilities, StringComparison.Ordinal);
        Assert.Contains("@media (prefers-reduced-motion: reduce)", utilities, StringComparison.Ordinal);
    }

    // Sprint 32.18: EditorialSectionNav.razor.css (the 12 institutional/footer pages' section nav)
    // was the remaining stylesheet from the current 8-file inventory not owned by Daily (32.15),
    // Wallet, or Auth/ProfileCreation (32.5, out of this Sprint's Public Pages scope) - a genuine
    // Public Pages gap, fixed here.
    [Fact]
    public void EditorialSectionNavHasALocalReducedMotionFallback()
    {
        var editorialSectionNav = ReadWebFile("Components", "Features", "Institutional", "Components", "EditorialSectionNav.razor.css");

        Assert.Contains("transition: border-color var(--beeday-transition-fast);", editorialSectionNav, StringComparison.Ordinal);
        Assert.Contains("@media (prefers-reduced-motion: reduce)", editorialSectionNav, StringComparison.Ordinal);
    }

    // EXP32-F011 (Sprint 32.19): editor-modal.css used to duplicate .beeday-field__control's exact
    // rest-state properties (width/min-height/padding/border/background/color/transition) under
    // .editor-modal__hero input/.editor-modal__field input/select - byte-for-byte redundant, since
    // every field inside an editor modal already carries .beeday-field__control by default
    // (BeeDayInput/BeeDaySelect/BeeDayDateInput/BeeDayTextArea's own InputCssClass default). Proves
    // the duplicate rule is gone while forms.css remains the single owner of that base styling; the
    // hover/focus overrides (a genuinely different, currently-live color treatment) are deliberately
    // left alone and not asserted against here.
    [Fact]
    public void EditorModalFieldsNoLongerDuplicateTheSharedFieldControlBaseStyling()
    {
        var editorModal = ReadWebFile("wwwroot", "css", "editor-modal.css");
        var forms = ReadWebFile("wwwroot", "css", "forms.css");

        Assert.Contains(".beeday-field__control {", forms, StringComparison.Ordinal);
        Assert.DoesNotContain(".editor-modal__hero input,", editorModal, StringComparison.Ordinal);
        Assert.DoesNotContain(".editor-modal__field input,", editorModal, StringComparison.Ordinal);
        Assert.DoesNotContain(".editor-modal__field select {", editorModal, StringComparison.Ordinal);
        // The editor-modal-specific textarea height override is an intentional divergence from
        // forms.css's 7rem default, not duplication - must survive the consolidation.
        Assert.Contains(".editor-modal__hero textarea {", editorModal, StringComparison.Ordinal);
        Assert.Contains("min-height: 5rem;", editorModal, StringComparison.Ordinal);
        // Hover/focus color treatment is intentionally untouched by this consolidation.
        Assert.Contains(".editor-modal__hero input:hover,", editorModal, StringComparison.Ordinal);
        Assert.Contains(".editor-modal__hero input:focus,", editorModal, StringComparison.Ordinal);
    }

    private static string ReadWebFile(params string[] segments) =>
        File.ReadAllText(Path.Combine([RepoRoot, "src", "BeeDay.Web", .. segments]));

    private static string ResolveRepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "BeeDay.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName
            ?? throw new InvalidOperationException("Could not locate the repository root from the test output directory.");
    }
}
