using System;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace ProgressDisplay;

/// <summary>
/// Логика взаимодействия для универсального окна прогресса.
/// </summary>
public partial class ProgressWindow : Window
{
    /// <summary>
    /// Событие нажатия на кнопку отмены.
    /// </summary>
    public event EventHandler? CancelRequested;

    /// <summary>
    /// Создаёт новый экземпляр класса <see cref="ProgressWindow"/>.
    /// </summary>
    public ProgressWindow()
    {
        InitializeComponent();
    }

    private void OnCancelClicked(object? sender, RoutedEventArgs e)
    {
        CancelRequested?.Invoke(this, EventArgs.Empty);
    }
}
