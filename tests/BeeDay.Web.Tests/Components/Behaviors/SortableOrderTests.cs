using LevelUp.Web.Components.Behaviors.DragDrop;

namespace LevelUp.Web.Tests.Components.Behaviors;

public sealed class SortableOrderTests
{
    [Fact]
    public void MovePlacesItemBeforeTarget()
    {
        var first = Guid.NewGuid();
        var second = Guid.NewGuid();
        var third = Guid.NewGuid();

        var result = SortableOrder.Move([first, second, third], third, first, placeAfter: false);

        Assert.Equal([third, first, second], result);
    }

    [Fact]
    public void MovePlacesItemAfterTarget()
    {
        var first = Guid.NewGuid();
        var second = Guid.NewGuid();
        var third = Guid.NewGuid();

        var result = SortableOrder.Move([first, second, third], first, second, placeAfter: true);

        Assert.Equal([second, first, third], result);
    }

    [Fact]
    public void MoveKeepsOrderWhenIdentifiersAreInvalid()
    {
        var first = Guid.NewGuid();
        var second = Guid.NewGuid();

        var result = SortableOrder.Move([first, second], Guid.NewGuid(), second, placeAfter: true);

        Assert.Equal([first, second], result);
    }
}
