namespace TodoList.Models;

public class TodoModel
{
    private string title = "";

    public string Title
    {
        get
        {
            return title;
        }

        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Название задачи не может быть пустым");
            }

            title = value;
        }
    }

    public string Description { get; set; }

    public TodoModel(string title, string description)
    {
        Title = title;
        Description = description;
    }
}