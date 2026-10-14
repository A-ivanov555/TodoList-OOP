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
    public TodoStatus Status { get; private set; } = TodoStatus.NotStarted;
    public TodoPriority Priority { get; private set; } 
    public DateTime CreatedAt { get; private set; } = DateTime.Now;
    public DateTime DueDate { get; private set; }
    
    public TodoModel(string title, string description, DateTime dueDate, TodoPriority priority=TodoPriority.Medium)
    {
        Title = title;
        Description = description;
        if (dueDate.Date < CreatedAt.Date)
        {
            throw new ArgumentException("Срок выполнения не может быть раньше даты создания");
        }
        DueDate = dueDate;
        
        Priority = priority;
        
    }

    public void ChangeStatus(TodoStatus newStatus)
    {
        Status = newStatus;
    }

    public void ChangePriority(TodoPriority newPriority)
    {
        Priority = newPriority;
    }

    public void ChangeDueDate(DateTime newDueDate)
    {
        if (newDueDate.Date < CreatedAt.Date)
        {
            throw new ArgumentException("Срок выполнения не может быть раньше даты создания");
        }
        DueDate = newDueDate;
    }

    
}