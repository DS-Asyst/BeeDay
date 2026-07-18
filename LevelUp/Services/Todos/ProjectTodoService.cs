using LevelUp.Domain.Milestones;
using LevelUp.Domain.Projects;
using LevelUp.Domain.Todos;

namespace LevelUp.Services.Todos;

public sealed class ProjectTodoService
{
    private readonly List<ProjectTodo> todos=[]; private int nextId=1;
    public ProjectTodoService(IEnumerable<ProjectTodo>? items=null){if(items is not null){todos.AddRange(items);if(todos.Count>0)nextId=todos.Max(x=>x.Id)+1;}}
    public ProjectTodo Create(Project project,string title,string description,Milestone? milestone=null)
    {
        ArgumentNullException.ThrowIfNull(project);
        if(milestone is not null && milestone.ProjectId!=project.Id)throw new InvalidOperationException("The milestone and to-do must belong to the same project.");
        var todo=new ProjectTodo{Id=nextId++};todo.Configure(project.Id,milestone?.Id,title,description,project.PrimaryAttribute);todos.Add(todo);return todo;
    }
    public IReadOnlyList<ProjectTodo> GetAll()=>todos.AsReadOnly();
    public IReadOnlyList<ProjectTodo> GetByProject(int projectId)=>todos.Where(x=>x.ProjectId==projectId).ToList().AsReadOnly();
    public ProjectTodo? GetById(int id)=>todos.FirstOrDefault(x=>x.Id==id);
    public void Activate(ProjectTodo todo){Ensure(todo);todo.Activate();}
    public void Complete(ProjectTodo todo){Ensure(todo);todo.Complete();}
    public bool Delete(int id){var t=GetById(id);if(t is null)return false;if(t.Status==TodoStatus.Completed)throw new InvalidOperationException("Completed to-dos cannot be deleted.");return todos.Remove(t);}
    private void Ensure(ProjectTodo todo){ArgumentNullException.ThrowIfNull(todo);if(!todos.Any(x=>x.Id==todo.Id))throw new InvalidOperationException("The to-do is not managed by this service.");}
}
