namespace LevelUp.Web.Components.Behaviors.DragDrop;

public sealed record SortableReorderEvent(
    string ItemId,
    string TargetItemId,
    bool PlaceAfter);
