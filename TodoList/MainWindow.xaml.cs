using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TodoList.Models;
using TodoList.Services;
using TodoList.Repositories;
namespace TodoList;


/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly TodoService _service = new();
    private readonly ITaskRepository _repository = new JsonTaskRepository();
    public MainWindow()
    {
        InitializeComponent();

        List<TodoModel> loadedTasks = _repository.Load();

        foreach (TodoModel task in loadedTasks)
        {
            _service.AddTask(task);
            TasksListBox.Items.Add(task);
        }
    }
    private void AddTaskButton_Click(object sender, RoutedEventArgs e)
    {
        if (DueDatePicker.SelectedDate == null)
        {
            MessageBox.Show("Выбери срок выполнения");
            return;
        }
        string title = TitleTextBox.Text;
        string description = DescriptionTextBox.Text;
        DateTime dueDate = DueDatePicker.SelectedDate.Value;
        TodoPriority priority = (TodoPriority)PriorityComboBox.SelectedIndex;
        
        TodoModel task = new TodoModel(title, description, dueDate, priority);
        _service.AddTask(task);
        TasksListBox.Items.Add(task);
        _repository.Save(_service.GetAllTasks());
    }
    private void DeleteTaskButton_Click(object sender, RoutedEventArgs e)
    {
        if (TasksListBox.SelectedItem is not TodoModel selectedTask)
        {
            MessageBox.Show("Сначала выбери задачу");
            return;
        }

        _service.RemoveTask(selectedTask);

        TasksListBox.Items.Remove(selectedTask);

        _repository.Save(_service.GetAllTasks());
    }
    private void ChangeStatusButton_Click(object sender, RoutedEventArgs e)
    {
        if (TasksListBox.SelectedItem is not TodoModel selectedTask)
        {
            MessageBox.Show("Сначала выбери задачу");
            return;
        }

        TodoStatus newStatus = (TodoStatus)StatusComboBox.SelectedIndex;

        _service.ChangeTaskStatus(selectedTask, newStatus);

        TasksListBox.Items.Refresh();

        _repository.Save(_service.GetAllTasks());
    }
    private void TasksListBox_SelectionChanged(
        object sender,
        SelectionChangedEventArgs e)
    {
        if (TasksListBox.SelectedItem is not TodoModel selectedTask)
        {
            TaskDetailsTextBlock.Text = "Выберите задачу из списка";
            return;
        }

        TaskDetailsTextBlock.Text =
            $"Название: {selectedTask.Title}\n" +
            $"Описание: {selectedTask.Description}\n" +
            $"Статус: {selectedTask.Status}\n" +
            $"Приоритет: {selectedTask.Priority}\n" +
            $"Создана: {selectedTask.CreatedAt:dd.MM.yyyy HH:mm}\n" +
            $"Срок выполнения: {selectedTask.DueDate:dd.MM.yyyy}";
    }
}