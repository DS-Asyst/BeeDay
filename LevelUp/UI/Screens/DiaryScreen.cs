namespace LevelUp.UI;

public sealed class DiaryScreen
{
    private readonly InputReader input;
    private readonly HabitScreen habits;
    private readonly TaskScreen tasks;
    private readonly TodoScreen todos;
    private readonly ProjectScreen projects;

    public DiaryScreen(InputReader input, HabitScreen habits, TaskScreen tasks, TodoScreen todos, ProjectScreen projects)
    {
        this.input = input;
        this.habits = habits;
        this.tasks = tasks;
        this.todos = todos;
        this.projects = projects;
    }

    public void Show()
    {
        while (true)
        {
            ConsoleHelper.ShowHeader("Diary");
            string option = input.ReadSelection("Choose a section:", new[] { "Habits", "Tasks", "To-Dos", "Projects", "Back" }, x => x);
            switch (option)
            {
                case "Habits": habits.Show(); break;
                case "Tasks": tasks.Show(); break;
                case "To-Dos": todos.Show(); break;
                case "Projects": projects.Show(); break;
                case "Back": return;
            }
        }
    }
}
