using LevelUp.Domain.Bosses;
using LevelUp.Domain.Milestones;
using LevelUp.Domain.Quests;
using LevelUp.UI.Components.Shared;
using LevelUp.UI.Infrastructure.Themes;
using Spectre.Console;

namespace LevelUp.UI.Components.Milestone;

public sealed class MilestoneCard
{
    private readonly Milestone milestone;
    private readonly IReadOnlyCollection<Quest> quests;
    private readonly BossEncounter? boss;
    private readonly decimal progress;

    public MilestoneCard(
        Milestone milestone,
        IEnumerable<Quest> quests,
        BossEncounter? boss,
        decimal progress
    )
    {
        this.milestone = milestone;
        this.quests = quests.ToList();
        this.boss = boss;
        this.progress = progress;
    }

    public Panel Build()
    {
        int completed = quests.Count(quest => quest.Status == QuestStatus.Completed);
        EntityCard card = new(milestone.Title, UIIcons.Milestone);

        card.AddText("Order", milestone.Order.ToString());
        card.AddMarkup("Status", MilestoneStatusFormatter.Format(milestone.Status));
        card.AddText("Description", milestone.Description);
        card.AddText("Progress", $"{progress:0.##}%", LevelUpTheme.Success);
        card.AddText("Quests", $"{completed}/{quests.Count}");
        card.AddText(
            "Requirement",
            milestone.RequiredCompletedQuests > 0
                ? $"Complete {milestone.RequiredCompletedQuests} quest(s)"
                : "Complete all linked quests"
        );

        if (milestone.Reward.HasReward)
        {
            card.AddText(
                "Reward",
                $"XP {milestone.Reward.Experience}, Gold {milestone.Reward.Gold}, Title {milestone.Reward.Title ?? "—"}",
                LevelUpTheme.Gold
            );
        }

        if (boss is not null)
        {
            card.AddText("Boss", $"{boss.Name} — {boss.Status}", LevelUpTheme.Boss);
        }

        return card.Build();
    }
}
