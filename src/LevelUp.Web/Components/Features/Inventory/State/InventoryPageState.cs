namespace LevelUp.Web.Components.Features.Inventory.State;

public sealed class InventoryPageState
{
    public string Search { get; set; } = string.Empty;
    public string TypeFilter { get; set; } = string.Empty;
    public string TagFilter { get; set; } = string.Empty;
    public string Sort { get; set; } = "date-desc";
    public int Page { get; set; } = 1;

    public bool HasFilters =>
        !string.IsNullOrWhiteSpace(Search) ||
        !string.IsNullOrWhiteSpace(TypeFilter) ||
        !string.IsNullOrWhiteSpace(TagFilter);

    public void ResetPage() => Page = 1;

    public void ClearFilters()
    {
        Search = string.Empty;
        TypeFilter = string.Empty;
        TagFilter = string.Empty;
        Page = 1;
    }
}
