using Microsoft.AspNetCore.Components;

namespace LevelUp.Web.Components.DesignSystem.Icons;

public partial class LevelUpIcon
{
    [Parameter, EditorRequired] public LevelUpIconName Name { get; set; }
    [Parameter] public int Size { get; set; } = 20;
    [Parameter] public bool Decorative { get; set; } = true;
    [Parameter] public string? Label { get; set; }
    [Parameter] public string? Class { get; set; }

    private string CssClasses => string.Join(' ', new[]
    {
        "levelup-icon",
        $"levelup-icon--{Name.ToString().ToLowerInvariant()}",
        Class
    }.Where(value => !string.IsNullOrWhiteSpace(value)));

    private IReadOnlyList<PixelRect> Pixels => Name switch
    {
        LevelUpIconName.Add => [R(10, 4, 4, 16), R(4, 10, 16, 4)],
        LevelUpIconName.Edit => [R(5, 15, 4, 4), R(8, 12, 4, 4), R(11, 9, 4, 4), R(14, 6, 4, 4), R(17, 5, 2, 4), R(5, 19, 8, 2)],
        LevelUpIconName.Delete => [R(6, 7, 12, 3), R(8, 10, 8, 10), R(9, 3, 6, 3), R(4, 5, 16, 2)],
        LevelUpIconName.Save => [R(4, 3, 16, 18), R(7, 4, 8, 6), R(8, 14, 8, 6), R(15, 5, 2, 4)],
        LevelUpIconName.Close => [R(5, 5, 4, 4), R(8, 8, 8, 8), R(15, 5, 4, 4), R(5, 15, 4, 4), R(15, 15, 4, 4)],
        LevelUpIconName.Search => [R(5, 5, 10, 3), R(5, 8, 3, 7), R(12, 8, 3, 7), R(8, 14, 7, 3), R(15, 15, 3, 3), R(18, 18, 3, 3)],
        LevelUpIconName.Settings => [R(9, 3, 6, 4), R(9, 17, 6, 4), R(3, 9, 4, 6), R(17, 9, 4, 6), R(7, 7, 10, 10), R(10, 10, 4, 4)],
        LevelUpIconName.User => [R(8, 3, 8, 8), R(5, 13, 14, 8)],
        LevelUpIconName.Lock => [R(7, 10, 10, 11), R(9, 5, 6, 7), R(10, 13, 4, 5)],
        LevelUpIconName.Language => [R(3, 5, 18, 3), R(10, 3, 4, 18), R(5, 11, 14, 3), R(5, 17, 14, 3)],
        LevelUpIconName.Check => [R(4, 11, 4, 4), R(7, 14, 4, 4), R(10, 11, 4, 4), R(13, 8, 4, 4), R(16, 5, 4, 4)],
        LevelUpIconName.ChevronDown => [R(5, 8, 4, 4), R(8, 11, 4, 4), R(12, 11, 4, 4), R(15, 8, 4, 4)],
        LevelUpIconName.ChevronLeft => [R(8, 5, 4, 4), R(5, 8, 4, 8), R(8, 15, 4, 4)],
        LevelUpIconName.ChevronRight => [R(12, 5, 4, 4), R(15, 8, 4, 8), R(12, 15, 4, 4)],
        LevelUpIconName.More => [R(3, 10, 4, 4), R(10, 10, 4, 4), R(17, 10, 4, 4)],
        LevelUpIconName.Warning => [R(10, 3, 4, 4), R(8, 7, 8, 8), R(6, 15, 12, 5), R(10, 10, 4, 5), R(10, 17, 4, 2)],
        LevelUpIconName.Info => [R(5, 3, 14, 18), R(10, 7, 4, 3), R(10, 12, 4, 6)],
        LevelUpIconName.Inventory => [R(4, 7, 16, 13), R(7, 4, 10, 4), R(7, 10, 10, 3)],
        LevelUpIconName.Book => [R(3, 4, 8, 16), R(13, 4, 8, 16), R(10, 6, 4, 14)],
        LevelUpIconName.Daily => [R(4, 5, 16, 16), R(7, 3, 3, 5), R(14, 3, 3, 5), R(7, 10, 3, 3), R(12, 10, 3, 3), R(7, 15, 3, 3)],
        _ => []
    };

    private static PixelRect R(double x, double y, double width, double height) => new(x, y, width, height);

    private sealed record PixelRect(double X, double Y, double Width, double Height);
}
