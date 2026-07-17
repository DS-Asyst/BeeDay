using LevelUp.UI.Components.Shared;
using LevelUp.UI.Infrastructure.Themes;
using Spectre.Console;
using QuestModel = LevelUp.Domain.Quests.Quest;

namespace LevelUp.UI.Components.Quest;

public sealed class QuestCard
{
    private readonly QuestModel quest;
    private readonly string projectName;

    public QuestCard(
        QuestModel quest,
        string projectName
    )
    {
        ArgumentNullException.ThrowIfNull(quest);

        this.quest = quest;
        this.projectName =
            string.IsNullOrWhiteSpace(projectName)
                ? "Independente"
                : projectName;
    }

    public Panel Build()
    {
        EntityCard card = new(
            quest.Title,
            UIIcons.Quest
        );

        card.AddText("ID", quest.Id.ToString());
        card.AddMarkup(
            "Status",
            QuestStatusFormatter.Format(quest.Status)
        );
        card.AddText(
            "Projeto",
            projectName,
            LevelUpTheme.Accent
        );
        card.AddText("Descrição", quest.Description);
        card.AddText(
            "Criada em",
            quest.CreatedAt.ToString("dd/MM/yyyy HH:mm")
        );

        if (quest.ActivatedAt is not null)
        {
            card.AddText(
                "Ativada em",
                quest.ActivatedAt.Value.ToString(
                    "dd/MM/yyyy HH:mm"
                )
            );
        }

        if (quest.CompletedAt is not null)
        {
            card.AddText(
                "Concluída em",
                quest.CompletedAt.Value.ToString(
                    "dd/MM/yyyy HH:mm"
                ),
                LevelUpTheme.Success
            );
        }

        if (quest.ArchivedAt is not null)
        {
            card.AddText(
                "Arquivada em",
                quest.ArchivedAt.Value.ToString(
                    "dd/MM/yyyy HH:mm"
                ),
                LevelUpTheme.MutedText
            );
        }

        return card.Build();
    }
}
