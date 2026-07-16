using LevelUp.Domain.Milestones;
using LevelUp.Domain.Quests;
using LevelUp.Services.Bosses;
using LevelUp.Services.Milestones;
using LevelUp.Services.Persistence;
using LevelUp.Services.Quests;

namespace LevelUp.Services.Workflows;

public sealed class MilestoneWorkflowService
{
    private readonly MilestoneService milestoneService;
    private readonly QuestService questService;
    private readonly BossService bossService;
    private readonly GameStateService gameStateService;

    public MilestoneWorkflowService(
        MilestoneService milestoneService,
        QuestService questService,
        BossService bossService,
        GameStateService gameStateService
    )
    {
        this.milestoneService = milestoneService;
        this.questService = questService;
        this.bossService = bossService;
        this.gameStateService = gameStateService;
    }

    public void CompleteManualMilestone(int milestoneId)
    {
        Milestone milestone = milestoneService.GetById(milestoneId)
            ?? throw new InvalidOperationException("The milestone was not found.");

        var boss = bossService.GetByMilestoneId(milestone.Id);
        if (boss is not null)
        {
            if (milestone.Status != MilestoneStatus.Active)
            {
                throw new InvalidOperationException(
                    "The milestone must be active before its boss can be unlocked."
                );
            }

            bossService.TryUnlockForMilestoneRequirement(milestone, requirementsMet: true);
        }
        else
        {
            milestoneService.CompleteManually(milestone, questService.GetAllQuests());
            milestoneService.UnlockAndActivateNext(milestone);
        }

        gameStateService.Save();
    }

    public bool DeleteMilestone(int milestoneId)
    {
        Milestone? milestone = milestoneService.GetById(milestoneId);
        if (milestone is null)
        {
            return false;
        }

        var linkedQuests = questService.GetQuestsByMilestoneId(milestoneId);
        if (linkedQuests.Any(quest => quest.Status is QuestStatus.Completed or QuestStatus.Archived))
        {
            throw new InvalidOperationException(
                "Milestones with completed or archived quests cannot be deleted. Archive the milestone instead."
            );
        }

        foreach (var quest in linkedQuests)
        {
            questService.RemoveQuestFromMilestone(quest);
        }

        bossService.DeleteByMilestoneId(milestoneId);
        bool deleted = milestoneService.Delete(milestoneId);

        if (deleted)
        {
            gameStateService.Save();
        }

        return deleted;
    }
}
