using LevelUp.Domain.Quests;
using LevelUp.UI.Components.Shared;
using LevelUp.UI.Infrastructure.Themes;
using Spectre.Console;
using MilestoneModel = LevelUp.Domain.Milestones.Milestone;
using QuestModel = LevelUp.Domain.Quests.Quest;

namespace LevelUp.UI.Components.Milestone;

public sealed class MilestoneCard
{
    private readonly MilestoneModel milestone;
    private readonly IReadOnlyCollection<QuestModel> quests;
    private readonly decimal progress;

    public MilestoneCard(
        MilestoneModel milestone,
        IEnumerable<QuestModel> quests,
        decimal progress
    )
    {
        ArgumentNullException.ThrowIfNull(milestone);
        ArgumentNullException.ThrowIfNull(quests);
        this.milestone = milestone;
        this.quests = quests.ToList();
        this.progress = progress;
    }

    public Panel Build()
    {
        int completed = quests.Count(quest => quest.Status == QuestStatus.Completed);
        EntityCard card = new(milestone.Title, UIIcons.Milestone);
        card.AddText("Ordem", milestone.Order.ToString());
        card.AddMarkup("Status", MilestoneStatusFormatter.Format(milestone.Status));
        card.AddText("Description", milestone.Description);
        card.AddText("Progress", $"{progress:0.##}%", LevelUpTheme.Success);
        card.AddText("Tasks", $"{completed}/{quests.Count}");
        card.AddText(
            "Requisito",
            milestone.RequiredCompletedQuests > 0
                ? $"Score Positive {milestone.RequiredCompletedQuests} task(s)"
                : "Score Positive todas as tasks vinculadas"
        );
        return card.Build();
    }
}
