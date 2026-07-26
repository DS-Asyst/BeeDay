using LevelUp.Domain.Enums;
using LevelUp.Web.Components.Features.Dashboard.Components;
using LevelUp.Web.Components.Features.Dashboard.State;

namespace LevelUp.Web.Tests.Components.Dashboard;

public sealed class FilterBarTests
{
    [Fact]
    public void RendersAttributeFilterAndSortControls()
    {
        using var context = new BunitContext();
        var cut = context.Render<FilterBar>(parameters => parameters
            .Add(component => component.Attribute, ActivityAttribute.Wisdom)
            .Add(component => component.Sort, ActivitySortOption.AttributeAscending)
            .Add(component => component.ResultCount, 2)
            .Add(component => component.TotalCount, 8));

        Assert.Equal("Wisdom", cut.Find("select[aria-label='Filter by attribute']").GetAttribute("value"));
        Assert.Contains("Attribute A–Z", cut.Markup);
        Assert.Contains("Search activities or attributes", cut.Markup);
    }

    [Fact]
    public async Task EmitsSelectedAttributeAndSortOption()
    {
        using var context = new BunitContext();
        ActivityAttribute? selectedAttribute = null;
        var selectedSort = ActivitySortOption.Manual;

        var cut = context.Render<FilterBar>(parameters => parameters
            .Add(component => component.AttributeChanged, value => selectedAttribute = value)
            .Add(component => component.SortChanged, value => selectedSort = value));

        await cut.Find("select[aria-label='Filter by attribute']").ChangeAsync(ActivityAttribute.Charisma.ToString());
        await cut.Find("select[aria-label='Sort activities']").ChangeAsync(ActivitySortOption.AttributeDescending.ToString());

        Assert.Equal(ActivityAttribute.Charisma, selectedAttribute);
        Assert.Equal(ActivitySortOption.AttributeDescending, selectedSort);
    }
}
