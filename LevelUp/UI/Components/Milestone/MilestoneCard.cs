using LevelUp.Domain.Bosses;
using LevelUp.UI.Components.Shared;
using LevelUp.UI.Infrastructure.Themes;
using Spectre.Console;
using MilestoneModel = LevelUp.Domain.Milestones.Milestone;
using QuestModel = LevelUp.Domain.Quests.Quest;
using LevelUp.UI.Infrastructure;

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

        card.AddText("Ordem", milestone.Order.ToString());
        card.AddMarkup(
            "Status",
            MilestoneStatusFormatter.Format(milestone.Status)
        );
        card.AddText("Descrição", milestone.Description);
        card.AddText(
            "Progresso",
            $"{progress:0.##}%",
            LevelUpTheme.Success
        );
        card.AddText("Missões", $"{completed}/{quests.Count}");
        card.AddText(
            "Requisito",
            milestone.RequiredCompletedQuests > 0
                ? $"Concluir {milestone.RequiredCompletedQuests} missão(ões)"
                : "Concluir todas as missões vinculadas"
        );

        if (milestone.Reward.HasReward)
        {
            card.AddText(
                "Recompensa",
                $"XP {milestone.Reward.Experience}, " +
                $"Título {milestone.Reward.Title ?? "—"}",
                LevelUpTheme.Gold
            );
        }

        if (boss is not null)
        {
            card.AddText(
                "Chefe",
                $"{boss.Name} — {DisplayText.For(boss.Status)}",
                LevelUpTheme.Boss
            );
        }

        return card.Build();
    }
}
