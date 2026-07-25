using LevelUp.Application.Features.Characters.Commands;
using LevelUp.Application.Features.Characters.Requests;
using LevelUp.Application.Features.Characters.Validation;
using LevelUp.Application.Features.Habits.Requests;
using LevelUp.Application.Features.Habits.Validation;
using LevelUp.Application.Features.Ordering.Requests;
using LevelUp.Application.Features.Ordering.Validation;
using LevelUp.Application.Features.Projects.Requests;
using LevelUp.Application.Features.Projects.Validation;
using LevelUp.Application.Features.Tasks.Requests;
using LevelUp.Application.Features.Tasks.Validation;
using LevelUp.Application.Features.Todos.Requests;
using LevelUp.Application.Features.Todos.Validation;
using LevelUp.Domain.Enums;

namespace LevelUp.Application.Tests;

public sealed class RequestValidatorTests
{
    [Fact]
    public async Task SaveHabit_RejectsEmptyTitle()
    {
        var result = await new SaveHabitRequestValidator().ValidateAsync(
            new SaveHabitRequest(" ", string.Empty, HabitDirection.Both, HabitDifficulty.Easy, HabitResetCounter.Daily),
            TestContext.Current.CancellationToken);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "Title");
    }

    [Fact]
    public async Task SaveTask_RejectsUndefinedRepeat()
    {
        var result = await new SaveTaskRequestValidator().ValidateAsync(
            new SaveTaskRequest("Task", string.Empty, (TaskRepeat)999),
            TestContext.Current.CancellationToken);
        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task SaveTodo_AcceptsOptionalDueDate()
    {
        var result = await new SaveTodoRequestValidator().ValidateAsync(
            new SaveTodoRequest(Guid.NewGuid(), "To-do", string.Empty, null),
            TestContext.Current.CancellationToken);
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task SaveProject_RejectsLongDescription()
    {
        var result = await new SaveProjectRequestValidator().ValidateAsync(
            new SaveProjectRequest("Project", new string('x', 501), "#7A4FCB", null, false),
            TestContext.Current.CancellationToken);
        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("ab")]
    [InlineData("invalid nickname")]
    public async Task CreateCharacter_RejectsInvalidNickname(string nickname)
    {
        var result = await new CreateCharacterCommandValidator().ValidateAsync(
            new CreateCharacterCommand(new CreateCharacterRequest("Tiago", nickname, CharacterClass.Warrior)),
            TestContext.Current.CancellationToken);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "Nickname");
    }

    [Fact]
    public async Task Reorder_RejectsDuplicateIdentifiers()
    {
        var id = Guid.NewGuid();
        var result = await new ReorderActivitiesRequestValidator().ValidateAsync(
            new ReorderActivitiesRequest(ActivityCollection.Tasks, [id, id]),
            TestContext.Current.CancellationToken);
        Assert.False(result.IsValid);
    }
}
