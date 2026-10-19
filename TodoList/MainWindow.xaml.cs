
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TodoList.Models;
using TodoList.Repositories;
using TodoList.Services;
using System.Windows.Media.Animation;
namespace TodoList;

public partial class MainWindow : Window
{
    private readonly TodoService _service = new();
    private readonly ITaskRepository _repository = new JsonTaskRepository();

    private TodoModel? _editingTask;
    private bool _editorOpen;
    private bool _darkTheme;

    public MainWindow()
    {
        InitializeComponent();

        try
        {
            foreach (TodoModel task in _repository.Load())
            {
                _service.AddTask(task);
            }
        }
        catch (Exception ex) when (ex is IOException
                                   or JsonException
                                   or UnauthorizedAccessException)
        {
            MessageBox.Show(
                $"Не удалось загрузить задачи:\n{ex.Message}",
                "Ошибка загрузки",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }

        RefreshTaskList();
    }

    // Обновление списка и поиск
    private void RefreshTaskList(TodoModel? taskToSelect = null)
    {
        string query = SearchTextBox.Text.Trim();

        TasksListBox.Items.Clear();

        foreach (TodoModel task in _service.GetAllTasks())
        {
            bool matches = task.Title.Contains(
                               query, StringComparison.OrdinalIgnoreCase)
                           || (task.Description ?? "").Contains(
                               query, StringComparison.OrdinalIgnoreCase);

            if (matches)
            {
                TasksListBox.Items.Add(task);
            }
        }

        int total = _service.GetTaskCount();

        TaskCountTextBlock.Text = string.IsNullOrWhiteSpace(query)
            ? $"({total})"
            : $"({TasksListBox.Items.Count} из {total})";

        if (taskToSelect != null && TasksListBox.Items.Contains(taskToSelect))
        {
            TasksListBox.SelectedItem = taskToSelect;
        }
    }

    // Переключение панелей справа
    private void ShowPanel(UIElement panel)
    {
        EmptyPanel.Visibility = Visibility.Collapsed;
        DetailsPanel.Visibility = Visibility.Collapsed;
        EditorPanel.Visibility = Visibility.Collapsed;

        panel.Visibility = Visibility.Visible;

        if (panel == EmptyPanel)
        {
            CloseRightPanel();
            return;
        }

        if (RightPanelContainer.Visibility == Visibility.Collapsed)
        {
            OpenRightPanel();
        }
    }
    private void OpenRightPanel()
    {
        TimeSpan duration = TimeSpan.FromMilliseconds(250);

        var easing = new CubicEase
        {
            EasingMode = EasingMode.EaseOut
        };

        RightPanelContainer.Visibility = Visibility.Visible;

        RightPanelContainer.Width = 415;
        RightPanelContainer.Opacity = 1;
        TasksContainer.Margin = new Thickness(0, 0, 16, 0);

        // Плавное увеличение ширины панели
        var widthAnimation = new DoubleAnimation(0, 415, duration)
        {
            EasingFunction = easing,
            FillBehavior = FillBehavior.Stop
        };

        RightPanelContainer.BeginAnimation(
            FrameworkElement.WidthProperty,
            widthAnimation);

        // Плавное появление панели
        var opacityAnimation = new DoubleAnimation(0, 1, duration)
        {
            FillBehavior = FillBehavior.Stop
        };

        RightPanelContainer.BeginAnimation(
            UIElement.OpacityProperty,
            opacityAnimation);

        // Плавное появление отступа между панелями
        var marginAnimation = new ThicknessAnimation(
            new Thickness(0),
            new Thickness(0, 0, 16, 0),
            duration)
        {
            EasingFunction = easing,
            FillBehavior = FillBehavior.Stop
        };

        TasksContainer.BeginAnimation(
            FrameworkElement.MarginProperty,
            marginAnimation);
    }
    private void CloseRightPanel()
    {
        RightPanelContainer.BeginAnimation(
            FrameworkElement.WidthProperty, null);

        RightPanelContainer.BeginAnimation(
            UIElement.OpacityProperty, null);

        TasksContainer.BeginAnimation(
            FrameworkElement.MarginProperty, null);

        RightPanelContainer.Width = 0;
        RightPanelContainer.Opacity = 0;
        RightPanelContainer.Visibility = Visibility.Collapsed;

        TasksContainer.Margin = new Thickness(0);
    }
    private void CloseDetailsButton_Click(object sender, RoutedEventArgs e)
    {
        TasksListBox.SelectedItem = null;
        CloseRightPanel();
    }

    // Отображение подробностей
    private void ShowTaskDetails(TodoModel task)
    {
        DetailTitleTextBlock.Text = task.Title;
        DetailDescriptionTextBlock.Text = string.IsNullOrWhiteSpace(task.Description)
            ? "Описание отсутствует"
            : task.Description;

        DetailStatusTextBlock.Text = task.Status switch
        {
            TodoStatus.NotStarted => "Не начата",
            TodoStatus.InProgress => "В процессе",
            TodoStatus.Completed => "Выполнена",
            _ => "Неизвестно"
        };

        DetailPriorityTextBlock.Text = task.Priority switch
        {
            TodoPriority.Low => "Низкий",
            TodoPriority.Medium => "Средний",
            TodoPriority.High => "Высокий",
            _ => "Неизвестно"
        };

        DetailCreatedTextBlock.Text =
            task.CreatedAt.ToString("dd.MM.yyyy HH:mm");

        DetailDueDateTextBlock.Text =
            task.DueDate.ToString("dd.MM.yyyy");

        ShowPanel(DetailsPanel);
    }

    // Выбор карточки задачи
    private void TasksListBox_SelectionChanged(
        object sender, SelectionChangedEventArgs e)
    {
        if (_editorOpen)
            return;

        if (TasksListBox.SelectedItem is TodoModel task)
        {
            ShowTaskDetails(task);
        }
        else
        {
            ShowPanel(EmptyPanel);
        }
    }

    // Создание новой задачи
    private void NewTaskButton_Click(object sender, RoutedEventArgs e)
    {
        _editingTask = null;
        _editorOpen = true;

        EditorHeadingTextBlock.Text = "Новая задача";

        EditTitleTextBox.Clear();
        EditDescriptionTextBox.Clear();
        EditDueDatePicker.SelectedDate = DateTime.Today;
        EditPriorityComboBox.SelectedIndex = 1;
        EditStatusComboBox.SelectedIndex = 0;

        ShowPanel(EditorPanel);
        EditTitleTextBox.Focus();
    }

    // Переход к редактированию выбранной задачи
    private void EditTaskButton_Click(object sender, RoutedEventArgs e)
    {
        if (TasksListBox.SelectedItem is not TodoModel task)
            return;

        _editingTask = task;
        _editorOpen = true;

        EditorHeadingTextBlock.Text = "Редактирование задачи";

        EditTitleTextBox.Text = task.Title;
        EditDescriptionTextBox.Text = task.Description;
        EditDueDatePicker.SelectedDate = task.DueDate;
        EditPriorityComboBox.SelectedIndex = (int)task.Priority;
        EditStatusComboBox.SelectedIndex = (int)task.Status;

        ShowPanel(EditorPanel);
    }

    // Сохранение новой или изменённой задачи
    private void SaveTaskButton_Click(object sender, RoutedEventArgs e)
    {
        string title = EditTitleTextBox.Text.Trim();
        string description = EditDescriptionTextBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(title))
        {
            MessageBox.Show("Введите название задачи.");
            return;
        }

        if (EditDueDatePicker.SelectedDate is not DateTime dueDate)
        {
            MessageBox.Show("Выберите срок выполнения.");
            return;
        }

        DateTime earliestDate = _editingTask?.CreatedAt.Date
                                ?? DateTime.Today;

        if (dueDate.Date < earliestDate)
        {
            MessageBox.Show(
                "Срок выполнения не может быть раньше даты создания.");
            return;
        }

        if (EditPriorityComboBox.SelectedIndex < 0
            || EditStatusComboBox.SelectedIndex < 0)
        {
            MessageBox.Show("Выберите приоритет и статус.");
            return;
        }

        TodoPriority priority =
            (TodoPriority)EditPriorityComboBox.SelectedIndex;

        TodoStatus status =
            (TodoStatus)EditStatusComboBox.SelectedIndex;

        TodoModel savedTask;

        try
        {
            if (_editingTask == null)
            {
                // Создаём новый объект задачи
                savedTask = new TodoModel(
                    title, description, dueDate, priority);

                _service.AddTask(savedTask);
                _service.ChangeTaskStatus(savedTask, status);
            }
            else
            {
                // Изменяем существующую задачу
                savedTask = _editingTask;

                _service.EditTask(savedTask, title, description);
                _service.ChangeTaskDueDate(savedTask, dueDate);
                _service.ChangeTaskPriority(savedTask, priority);
                _service.ChangeTaskStatus(savedTask, status);
            }
        }
        catch (ArgumentException ex)
        {
            MessageBox.Show(ex.Message, "Ошибка ввода");
            return;
        }

        SaveTasks();

        _editingTask = null;
        _editorOpen = false;

        // Показываем сохранённую задачу, даже если был активен поиск
        SearchTextBox.Clear();
        RefreshTaskList(savedTask);
        ShowTaskDetails(savedTask);
    }

    // Отмена создания или редактирования
    private void CancelEditButton_Click(object sender, RoutedEventArgs e)
    {
        _editingTask = null;
        _editorOpen = false;

        if (TasksListBox.SelectedItem is TodoModel task)
        {
            ShowTaskDetails(task);
        }
        else
        {
            ShowPanel(EmptyPanel);
        }
    }

    // Удаление задачи
    private void DeleteTaskButton_Click(object sender, RoutedEventArgs e)
    {
        if (TasksListBox.SelectedItem is not TodoModel task)
            return;

        MessageBoxResult answer = MessageBox.Show(
            $"Удалить задачу «{task.Title}»?",
            "Подтверждение удаления",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (answer != MessageBoxResult.Yes)
            return;

        _service.RemoveTask(task);

        SaveTasks();
        RefreshTaskList();
        ShowPanel(EmptyPanel);
    }

    // Сохранение всех задач в JSON
    private void SaveTasks()
    {
        try
        {
            _repository.Save(_service.GetAllTasks());
        }
        catch (Exception ex) when (ex is IOException
                                   or UnauthorizedAccessException)
        {
            MessageBox.Show(
                $"Не удалось сохранить задачи:\n{ex.Message}",
                "Ошибка сохранения",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    // Открытие и закрытие строки поиска
    private void SearchToggleButton_Click(object sender, RoutedEventArgs e)
    {
        if (SearchPanel.Visibility == Visibility.Collapsed)
        {
            SearchPanel.Visibility = Visibility.Visible;
            SearchTextBox.Focus();
        }
        else
        {
            SearchPanel.Visibility = Visibility.Collapsed;
            SearchTextBox.Clear();
        }
    }

    // Поиск при вводе текста
    private void SearchTextBox_TextChanged(
        object sender, TextChangedEventArgs e)
    {
        if (TasksListBox == null)
            return;

        RefreshTaskList();
    }

    // Переключение светлой и тёмной темы
    private void ThemeToggleButton_Click(object sender, RoutedEventArgs e)
    {
        _darkTheme = !_darkTheme;

        if (_darkTheme)
        {
            ChangeBrush("AppBackgroundBrush", "#151B2C");
            ChangeBrush("SurfaceBrush", "#202A3D");
            ChangeBrush("MainTextBrush", "#F2F4FC");
            ChangeBrush("MutedTextBrush", "#A0ADC5");
            ChangeBrush("AccentBrush", "#8983FF");
            ChangeBrush("AccentSoftBrush", "#30365B");
            ChangeBrush("BorderColorBrush", "#35405B");
            ChangeBrush("FieldBrush", "#29354B");
            ChangeBrush("SuccessBrush", "#43D7A4");
            ChangeBrush("DangerBrush", "#FF7888");

            ThemeToggleButton.Content = "💡 Светлая тема";
        }
        else
        {
            ChangeBrush("AppBackgroundBrush", "#F3F5FB");
            ChangeBrush("SurfaceBrush", "#FFFFFF");
            ChangeBrush("MainTextBrush", "#1C2644");
            ChangeBrush("MutedTextBrush", "#718096");
            ChangeBrush("AccentBrush", "#554DE7");
            ChangeBrush("AccentSoftBrush", "#ECEBFF");
            ChangeBrush("BorderColorBrush", "#E2E7F0");
            ChangeBrush("FieldBrush", "#F8F9FD");
            ChangeBrush("SuccessBrush", "#179C77");
            ChangeBrush("DangerBrush", "#DC4557");

            ThemeToggleButton.Content = "☾ Тёмная тема";
        }
    }

    private void ChangeBrush(string resourceKey, string colorCode)
    {
        Color color = (Color)ColorConverter.ConvertFromString(colorCode)!;

        Application.Current.Resources[resourceKey] =
            new SolidColorBrush(color);
    }
}
