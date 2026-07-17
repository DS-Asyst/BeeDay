using LevelUp.Domain.Milestones;
using LevelUp.Domain.Projects;
using LevelUp.Domain.Quests;
using LevelUp.Services.Milestones;
using LevelUp.Services.Projects;
using LevelUp.UI.Infrastructure;
using QuestModel = LevelUp.Domain.Quests.Quest;

namespace LevelUp.UI.Flows.Quests;

public sealed class QuestSelectionFlow
{
    private readonly ProjectService projectService;
    private readonly MilestoneService milestoneService;
    private readonly InputReader inputReader;

    public QuestSelectionFlow(
        ProjectService projectService,
        MilestoneService milestoneService,
        InputReader inputReader
    )
    {
        this.projectService = projectService;
        this.milestoneService = milestoneService;
        this.inputReader = inputReader;
    }

    public Project? SelectOptionalProject()
    {
        List<Project> projects = GetAvailableProjects();

        if (projects.Count == 0)
        {
            ConsoleHelper.ShowInformation(
                "Nenhum projeto disponível. A missão será independente."
            );
            return null;
        }

        if (!inputReader.ReadConfirmation(
            "Deseja associar esta missão a um projeto?"
        ))
        {
            return null;
        }

        return SelectProject("Selecione o projeto:", projects);
    }

    public Project? SelectOptionalProjectForCreation()
    {
        List<Project> projects = GetAvailableProjects();

        if (projects.Count == 0)
        {
            ConsoleHelper.ShowInformation(
                "Nenhum projeto disponível. A missão será independente."
            );
            return null;
        }

        PromptDecision decision = inputReader.ReadDecision(
            "Deseja associar esta missão a um projeto?"
        );

        if (decision == PromptDecision.Cancel)
        {
            throw new UserCancelledException();
        }

        if (decision == PromptDecision.No)
        {
            return null;
        }

        List<ProjectCreationChoice> choices = projects
            .Select(project => new ProjectCreationChoice(
                project,
                FormatProject(project)
            ))
            .ToList();

        choices.Add(new ProjectCreationChoice(null, "Cancelar"));

        ProjectCreationChoice selected = inputReader.ReadSelection(
            "Selecione o projeto:",
            choices,
            choice => choice.Label
        );

        return selected.Project ?? throw new UserCancelledException();
    }

    public Milestone? SelectOptionalMilestone(
        Project project,
        bool requireConfirmation = true
    )
    {
        List<Milestone> milestones = GetAvailableMilestones(project);

        if (milestones.Count == 0)
        {
            return null;
        }

        if (requireConfirmation && !inputReader.ReadConfirmation(
            "Associar esta missão a um capítulo?"
        ))
        {
            return null;
        }

        return inputReader.ReadSelection(
            "Selecione o capítulo:",
            milestones,
            FormatMilestone
        );
    }

    public Milestone? SelectOptionalMilestoneForCreation(Project project)
    {
        List<Milestone> milestones = GetAvailableMilestones(project);

        if (milestones.Count == 0)
        {
            return null;
        }

        PromptDecision decision = inputReader.ReadDecision(
            "Deseja associar esta missão a um capítulo?"
        );

        if (decision == PromptDecision.Cancel)
        {
            throw new UserCancelledException();
        }

        if (decision == PromptDecision.No)
        {
            return null;
        }

        List<MilestoneCreationChoice> choices = milestones
            .Select(milestone => new MilestoneCreationChoice(
                milestone,
                FormatMilestone(milestone)
            ))
            .ToList();

        choices.Add(new MilestoneCreationChoice(null, "Cancelar"));

        MilestoneCreationChoice selected = inputReader.ReadSelection(
            "Selecione o capítulo:",
            choices,
            choice => choice.Label
        );

        return selected.Milestone ?? throw new UserCancelledException();
    }

    public QuestModel SelectQuest(
        string prompt,
        IEnumerable<QuestModel> quests
    )
    {
        return inputReader.ReadSelection(
            prompt,
            quests,
            quest =>
                $"{quest.Title} — {DisplayText.For(quest.Status)} — " +
                GetProjectName(quest.ProjectId)
        );
    }

    public Project SelectProject(
        string prompt,
        IEnumerable<Project> projects
    )
    {
        return inputReader.ReadSelection(
            prompt,
            projects,
            FormatProject
        );
    }

    public string GetProjectName(int? projectId)
    {
        if (projectId is null)
        {
            return "Independente";
        }

        return projectService.GetProjectById(projectId.Value)?.Name
            ?? "Projeto não encontrado";
    }

    private List<Project> GetAvailableProjects()
    {
        return projectService
            .GetAllProjects()
            .Where(project =>
                project.Status is ProjectStatus.Created or
                    ProjectStatus.Active
            )
            .ToList();
    }

    private List<Milestone> GetAvailableMilestones(Project project)
    {
        return milestoneService
            .GetByProjectId(project.Id)
            .Where(milestone => milestone.CanAcceptQuests)
            .ToList();
    }

    private static string FormatProject(Project project)
    {
        return $"{project.Name} — {DisplayText.For(project.Status)}";
    }

    private static string FormatMilestone(Milestone milestone)
    {
        return $"{milestone.Order}. {milestone.Title} — " +
            DisplayText.For(milestone.Status);
    }

    private sealed record ProjectCreationChoice(
        Project? Project,
        string Label
    );

    private sealed record MilestoneCreationChoice(
        Milestone? Milestone,
        string Label
    );
}
