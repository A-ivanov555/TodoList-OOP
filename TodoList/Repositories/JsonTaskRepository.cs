using System.IO;
using TodoList.Models;
using System.Text.Json;

namespace TodoList.Repositories;

public class JsonTaskRepository :ITaskRepository
{
    public void Save(List<TodoModel> tasks)
    {
        string json = JsonSerializer.Serialize(tasks);
        File.WriteAllText("tasks.json", json);
    }

    public List<TodoModel> Load()
    {
        if (!File.Exists("tasks.json"))
        {
            return new List<TodoModel>();
        }
        string json = File.ReadAllText("tasks.json");
        List<TodoModel>? result= JsonSerializer.Deserialize<List<TodoModel>>(json);
        return result ?? new List<TodoModel>();
        
    }
    
}