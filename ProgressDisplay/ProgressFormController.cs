using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;

namespace ProgressDisplay;

/// <summary>
/// Предоставляет API для отображения и управления универсальным окном прогресса.
/// </summary>
public sealed class ProgressFormController
{
    private readonly object _syncRoot = new();
    private ProgressWindow? _window;
    private ProgressWindowViewModel? _viewModel;
    private ProgressCompletionMode _completionMode;
    private double _currentValue;
    private double _maximum;
    private bool _cancelRequested;
    private bool _completionRaised;
    private bool _cancelRaised;

    /// <summary>
    /// Событие, генерируемое при необходимости отменить выполняемую операцию.
    /// </summary>
    public event EventHandler? OperationCancelled;

    /// <summary>
    /// Событие, возникающее при закрытии формы после достижения максимального числа записей.
    /// </summary>
    public event EventHandler? FormClosed;

    /// <summary>
    /// Инициализирует и отображает форму прогресса.
    /// </summary>
    /// <param name="title">Заголовок окна.</param>
    /// <param name="caption">Подпись, отображаемая внутри формы.</param>
    /// <param name="maximum">Максимальное количество обрабатываемых записей.</param>
    /// <param name="completionMode">Определяет поведение после достижения максимального значения.</param>
    public void Initialize(string title, string caption, double maximum, ProgressCompletionMode completionMode)
    {
        if (maximum <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maximum));
        }

        _completionMode = completionMode;
        _maximum = maximum;
        _currentValue = 0;
        _cancelRequested = false;
        _completionRaised = false;
        _cancelRaised = false;

        Dispatcher.UIThread.Post(() =>
        {
            _viewModel = new ProgressWindowViewModel
            {
                Caption = caption,
                Details = FormatDetails(0, maximum),
                ProgressMaximum = maximum,
                ProgressValue = 0,
                CancelButtonText = "Отмена",
                IsCancelEnabled = true
            };

            _viewModel.CancellationRequested += OnCancellationRequested;

            _window = new ProgressWindow
            {
                DataContext = _viewModel,
                Title = title,
                CanResize = false,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            _window.Closing += OnWindowClosing;
            _window.Closed += OnWindowClosed;

            var lifetime = Application.Current?.ApplicationLifetime;
            if (lifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime && desktopLifetime.MainWindow != null)
            {
                _window.Show(desktopLifetime.MainWindow);
            }
            else
            {
                _window.Show();
            }
        });
    }

    /// <summary>
    /// Передаёт форме сведения о продвижении выполнения.
    /// </summary>
    /// <param name="processedCount">Количество обработанных записей. По умолчанию — 1.</param>
    /// <param name="newCaption">Необязательная новая подпись.</param>
    /// <returns><c>true</c>, если вызывающему коду следует прекратить обработку; иначе <c>false</c>.</returns>
    public bool ReportProgress(double processedCount = 1, string? newCaption = null)
    {
        if (processedCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(processedCount));
        }

        lock (_syncRoot)
        {
            _currentValue = Math.Min(_currentValue + processedCount, _maximum);
        }

        Dispatcher.UIThread.Post(() =>
        {
            if (_viewModel is null)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(newCaption))
            {
                _viewModel.Caption = newCaption;
            }

            _viewModel.ProgressValue = _currentValue;
            _viewModel.Details = FormatDetails(_currentValue, _maximum);

            if (_currentValue >= _maximum)
            {
                HandleCompletionReached();
            }
        });

        return _cancelRequested;
    }

    /// <summary>
    /// Закрывает форму прогресса.
    /// </summary>
    public void Close()
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (_window != null)
            {
                _window.Close();
            }
        });
    }

    private void HandleCompletionReached()
    {
        if (_viewModel is null)
        {
            return;
        }

        _viewModel.CancelButtonText = "Закрыть";

        if (_completionMode == ProgressCompletionMode.WaitForUserAction)
        {
            _viewModel.IsCancelEnabled = true;
        }
        else
        {
            _viewModel.IsCancelEnabled = false;
            Close();
        }
    }

    private void OnCancellationRequested(object? sender, EventArgs e)
    {
        if (_viewModel is null)
        {
            return;
        }

        if (_currentValue >= _maximum)
        {
            Close();
            return;
        }

        _cancelRequested = true;
        _viewModel.IsCancelEnabled = false;
        RaiseCancellation();
    }

    private void OnWindowClosing(object? sender, WindowClosingEventArgs e)
    {
        if (_currentValue >= _maximum)
        {
            return;
        }

        if (!_cancelRequested)
        {
            _cancelRequested = true;
            RaiseCancellation();
        }
    }

    private void OnWindowClosed(object? sender, EventArgs e)
    {
        if (_viewModel != null)
        {
            _viewModel.IsCancelEnabled = false;
        }

        if (_currentValue >= _maximum)
        {
            RaiseCompletionEvent();
        }
        else
        {
            RaiseCancellation();
        }

        if (_window != null)
        {
            _window.Closing -= OnWindowClosing;
            _window.Closed -= OnWindowClosed;
        }

        if (_viewModel != null)
        {
            _viewModel.CancellationRequested -= OnCancellationRequested;
        }

        _window = null;
        _viewModel = null;
    }

    private void RaiseCompletionEvent()
    {
        if (_completionRaised)
        {
            return;
        }

        _completionRaised = true;
        FormClosed?.Invoke(this, EventArgs.Empty);
    }

    private void RaiseCancellation()
    {
        if (_cancelRaised)
        {
            return;
        }

        _cancelRaised = true;
        OperationCancelled?.Invoke(this, EventArgs.Empty);
    }

    private static string FormatDetails(double current, double maximum)
    {
        return $"Обработано {current:N0} из {maximum:N0}";
    }
}
