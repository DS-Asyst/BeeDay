using LevelUp.Domain.Bosses;
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
    private readonly BossEncounter? boss;
    private readonly decimal progress;

    public MilestoneCard(
        MilestoneModel milestone,
        IEnumerable<QuestModel> quests,
        BossEncounter? boss,
        decimal progress
    )
    {
        ArgumentNullException.ThrowIfNull(milestone);
        ArgumentNullException.ThrowIfNull(quests);

        this.milestone = milestone;
        this.quests = quests.ToList();
        this.boss = boss;
        this.progress = progress;
    }

    public Panel Build()
    {
        int completed = quests.Count(
            quest => quest.Status == Domain.Quests.QuestStatus.Completed
        );

        EntityCard card = new(
            milestone.Title,
            UIIcons.Milestone
        );

        card.AddText("Order", milestone.Order.ToString());
        card.AddMarkup(
            "Status",
            MilestoneStatusFormatter.Format(milestone.Status)
        );
        card.AddText("Description", milestone.Description);
        card.AddText(
            "Progress",
            $"{progress:0.##}%",
            LevelUpTheme.Success
        );
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
                $"XP {milestone.Reward.Experience}, " +
                $"Gold {milestone.Reward.Gold}, " +
                $"Title {milestone.Reward.Title ?? "—"}",
                LevelUpTheme.Gold
            );
        }

        if (boss is not null)
        {
            card.AddText(
                "Boss",
                $"{boss.Name} — {boss.Status}",
                LevelUpTheme.Boss
            );
        }

        return card.Build();
    }
}
