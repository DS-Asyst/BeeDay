namespace BeeDay.Web.Components.Features.Institutional;

/// <summary>
/// Sprint 29.4 — the single source of truth for the editorial header's contextual navigation
/// ("route -&gt; editorial section registry -&gt; section -&gt; navigation items"). Mirrors
/// AppFooter.razor's own groups exactly, one entry per footer link, and reuses the same
/// SharedResources label keys the footer already renders — no translation is duplicated or
/// hardcoded here. Centralized so no page or component re-declares its family's sibling routes.
/// </summary>
public static class EditorialSectionRegistry
{
    private static readonly IReadOnlyDictionary<EditorialSection, IReadOnlyList<EditorialSectionLink>> Sections =
        new Dictionary<EditorialSection, IReadOnlyList<EditorialSectionLink>>
        {
            [EditorialSection.AboutUs] =
            [
                new EditorialSectionLink("FooterMissionLink", "/mission"),
                new EditorialSectionLink("FooterEfficacyLink", "/efficacy"),
                new EditorialSectionLink("FooterBrandGuidelinesLink", "/brand-guidelines"),
                new EditorialSectionLink("FooterContactLink", "/contact")
            ],
            [EditorialSection.Products] =
            [
                new EditorialSectionLink("FooterProductLink", "/beeday"),
                new EditorialSectionLink("FooterProductPlusLink", "/beeday-plus")
            ],
            [EditorialSection.Apps] =
            [
                new EditorialSectionLink("FooterAndroidLink", "/android"),
                new EditorialSectionLink("FooterIosLink", "/ios")
            ],
            [EditorialSection.Help] =
            [
                new EditorialSectionLink("FooterFaqsLink", "/faqs")
            ],
            [EditorialSection.Legal] =
            [
                new EditorialSectionLink("FooterCommunityGuidelinesLink", "/community-guidelines"),
                new EditorialSectionLink("FooterTermsLink", "/terms"),
                new EditorialSectionLink("FooterPrivacyLink", "/privacy")
            ]
        };

    /// <summary>The family's sibling links. A single-page family (Help) still returns its one link.</summary>
    public static IReadOnlyList<EditorialSectionLink> GetLinks(EditorialSection section) => Sections[section];

    /// <summary>
    /// The InstitutionalResources key for the family's eyebrow text, used as the editorial hero's
    /// eyebrow above the H1. These five keys (AboutUsEyebrow, ProductsEyebrow, AppsEyebrow,
    /// HelpEyebrow, LegalEyebrow) already existed, already localized pt-BR/en-US, unused by any page
    /// before Sprint 29.4 — this wires them up instead of introducing new translations.
    /// </summary>
    public static string GetFamilyEyebrowResourceKey(EditorialSection section) => section switch
    {
        EditorialSection.AboutUs => "AboutUsEyebrow",
        EditorialSection.Products => "ProductsEyebrow",
        EditorialSection.Apps => "AppsEyebrow",
        EditorialSection.Help => "HelpEyebrow",
        EditorialSection.Legal => "LegalEyebrow",
        _ => throw new ArgumentOutOfRangeException(nameof(section))
    };
}
