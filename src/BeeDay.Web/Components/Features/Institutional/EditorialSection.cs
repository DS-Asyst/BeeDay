namespace BeeDay.Web.Components.Features.Institutional;

/// <summary>
/// The footer-derived families that group the public editorial pages (Sprint 29.4). Membership
/// mirrors AppFooter.razor's own groups exactly — this is not a separate taxonomy, it is the
/// deterministic key <see cref="EditorialSectionRegistry"/> uses to resolve each family's contextual
/// navigation. Social links are intentionally not represented: they are external and never become
/// editorial pages.
/// </summary>
public enum EditorialSection
{
    AboutUs,
    Products,
    Apps,
    Help,
    Legal
}
