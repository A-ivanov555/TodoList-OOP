using TodoList.Models;

namespace TodoList.Repositories;

public interface ITaskRepository
{
    void Save(List<TodoModel> tasks);
    List<TodoModel> Load();
    
    
}