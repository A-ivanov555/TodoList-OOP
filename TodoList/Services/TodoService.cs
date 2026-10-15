using TodoList.Models;

namespace TodoList.Services;

public class TodoService
{
    private readonly List<TodoModel> tasks = new();

    public void AddTask(TodoModel task)
    {
        tasks.Add(task);
    }
    public bool RemoveTask(TodoModel task)
    {
        return tasks.Remove(task);
        
    }

    public int GetTaskCount()
    {
        return tasks.Count;
    }

    public List<TodoModel> GetAllTasks()
    {
        return new List<TodoModel>(tasks);
    }

    public List<TodoModel> GetTasksByStatus(TodoStatus status)
    {
        List<TodoModel> result = new();
        foreach (var task in tasks)
        {
            if (task.Status == status)
            {
                result.Add(task);
            }
            
        }
        return result;
    }

    public bool ChangeTaskStatus(TodoModel task, TodoStatus newstatus)
    {
        if (!tasks.Contains(task))
        {
            return false;
        }
        task.ChangeStatus(newstatus);
        return true;
    }

    public bool EditTask(TodoModel task, string newTitle, string newDescription)
    {
        if (!tasks.Contains(task))
        {
            return false;
        }
        task.Title = newTitle;
        task.Description = newDescription;
        return true;
    }

    public bool ChangeTaskPriority(TodoModel task, TodoPriority newPriority)
    {
        if (!tasks.Contains(task))
        {
            return false;
        }
        task.ChangePriority(newPriority);
        return true;
    }

    public bool ChangeTaskDueDate(TodoModel task, DateTime newDueDate)
    {
        if (!tasks.Contains(task))
        {
            return false;
        }
        task.ChangeDueDate(newDueDate);
        return true;
    }
}