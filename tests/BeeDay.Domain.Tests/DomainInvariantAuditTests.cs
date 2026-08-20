using BeeDay.Domain.Entities;
using BeeDay.Domain.Enums;
using BeeDay.Domain.Exceptions;
using BeeDay.Domain.Experience;
using BeeDay.Domain.ValueObjects;
using Xunit;

namespace BeeDay.Domain.Tests;

public sealed class DomainInvariantAuditTests
{
    private const decimal SupportedMaximumTransactionAmount = 999999999999m;

    public static TheoryData<Type> FactoryControlledTypes => new()
    {
        typeof(User),
        typeof(UserToken),
        typeof(Habit),
        typeof(RecurringTask),
        typeof(Project),
        typeof(Todo),
        typeof(Wallet),
        typeof(WalletTag),
        typeof(Transaction),
        typeof(ExperienceEntry),
        typeof(ExperienceSource),
        typeof(UserExperience),
    };

    [Theory]
    [MemberData(nameof(FactoryControlledTypes))]
    public void FactoryControlledType_DoesNotExposePublicConstructor(Type type) =>
        Assert.Empty(type.GetConstructors());

    [Fact]
    public void ActivityOwner_CannotBeReassigned()
    {
        var habit = Habit.Create("Walk", null, HabitDirection.Both, HabitDifficulty.Easy, HabitResetCounter.Daily);
        habit.AssignOwner(Guid.NewGuid());

        Assert.Throws<InvalidDomainStateException>(() => habit.AssignOwner(Guid.NewGuid()));
    }

    [Fact]
    public void TodoProject_CanOnlyChangeThroughTheOwningProject()
    {
        var originalProjectId = Guid.NewGuid();
        var todo = Todo.Create(originalProjectId, "Write tests", null, null);

        Assert.Throws<InvalidDomainStateException>(() =>
            todo.Update(Guid.NewGuid(), "Write tests", null, null));
    }

    [Fact]
    public void Project_RejectsTodoOwnedByAnotherUser()
    {
        var project = Project.Create("Project", null);
        project.AssignOwner(Guid.NewGuid());
        var todo = Todo.Create(project.Id, "Todo", null, null);
        todo.AssignOwner(Guid.NewGuid());

        Assert.Throws<InvalidDomainStateException>(() => project.AddTodo(todo));
    }

    [Fact]
    public void Project_RejectsTheSameTodoTwice()
    {
        var project = Project.Create("Project", null);
        var todo = Todo.Create(project.Id, "Todo", null, null);
        project.AddTodo(todo);

        Assert.Throws<InvalidDomainStateException>(() => project.AddTodo(todo));
    }

    [Fact]
    public void UserToken_CannotBeUsedBeforeCreation()
    {
        var createdAtUtc = new DateTimeOffset(2026, 8, 20, 12, 0, 0, TimeSpan.Zero);
        var token = UserToken.Create(
            Guid.NewGuid(),
            UserTokenType.EmailConfirmation,
            "hash",
            createdAtUtc,
            createdAtUtc.AddHours(1));

        Assert.Throws<InvalidDomainStateException>(() =>
            token.MarkAsUsed(UserTokenType.EmailConfirmation, createdAtUtc.AddTicks(-1)));
    }

    [Fact]
    public void UserToken_RejectsDefaultCreationTimestamp() =>
        Assert.Throws<DomainValidationException>(() => UserToken.Create(
            Guid.NewGuid(),
            UserTokenType.EmailConfirmation,
            "hash",
            default,
            new DateTimeOffset(2026, 8, 20, 12, 0, 0, TimeSpan.Zero)));

    [Fact]
    public void User_SessionVersionCannotWrapAround()
    {
        var user = User.Create(
            "Alice",
            "alice@beeday.invalid",
            createdAtUtc: new DateTimeOffset(2026, 8, 20, 12, 0, 0, TimeSpan.Zero),
            passwordHash: null);
        typeof(User).GetProperty(nameof(User.SessionVersion))!.SetValue(user, int.MaxValue);

        Assert.Throws<OverflowException>(user.InvalidateSessions);
    }

    [Fact]
    public void EmailAddress_RejectsDisplayNameSyntax() =>
        Assert.Throws<DomainValidationException>(() =>
            EmailAddress.Create("Alice <alice@beeday.invalid>"));

    [Fact]
    public void ExperienceSource_UsesValueEquality()
    {
        var referenceId = Guid.NewGuid();

        var first = ExperienceSource.Create(ExperienceSourceType.Project, referenceId, "Completed");
        var second = ExperienceSource.Create(ExperienceSourceType.Project, referenceId, "Completed");

        Assert.Equal(first, second);
    }

    [Fact]
    public void ExperienceEntry_RejectsInconsistentExperienceTransition()
    {
        var source = ExperienceSource.Create(ExperienceSourceType.System);

        Assert.Throws<DomainValidationException>(() => ExperienceEntry.Create(
            Guid.NewGuid(),
            ExperienceReward.Create(10),
            source,
            ExperienceRewardType.Completion,
            experienceBefore: 0,
            experienceAfter: 5,
            levelBefore: 1,
            levelAfter: 1));
    }

    [Fact]
    public void Transaction_RejectsAmountAboveTheSupportedBusinessMaximum() =>
        Assert.Throws<DomainValidationException>(() => Transaction.Create(
            Guid.NewGuid(),
            "Unrepresentable amount",
            SupportedMaximumTransactionAmount + 0.01m,
            TransactionType.Income,
            new DateOnly(2026, 8, 20)));

    [Fact]
    public void Transaction_AcceptsTheSupportedBusinessMaximum()
    {
        var transaction = Transaction.Create(
            Guid.NewGuid(),
            "Maximum supported amount",
            Transaction.MaximumAmount,
            TransactionType.Income,
            new DateOnly(2026, 8, 20));

        Assert.Equal(SupportedMaximumTransactionAmount, transaction.Amount);
    }
}
