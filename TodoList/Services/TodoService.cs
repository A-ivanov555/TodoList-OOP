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
}