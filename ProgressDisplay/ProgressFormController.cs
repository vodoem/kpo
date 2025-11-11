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
    /// <summary>
    /// Объект синхронизации для обновления состояния прогресса из разных потоков.
    /// </summary>
    private readonly object _syncRoot = new();

    /// <summary>
    /// Текущее окно прогресса.
    /// </summary>
    private ProgressWindow? _window;

    /// <summary>
    /// Модель представления, отображающая состояние прогресса.
    /// </summary>
    private ProgressWindowViewModel? _viewModel;

    /// <summary>
    /// Выбранный режим завершения работы формы.
    /// </summary>
    private ProgressCompletionMode _completionMode;

    /// <summary>
    /// Текущее значение прогресса.
    /// </summary>
    private double _currentValue;

    /// <summary>
    /// Максимальное значение прогресса.
    /// </summary>
    private double _maximum;

    /// <summary>
    /// Признак того, что пользователь запросил отмену операции.
    /// </summary>
    private bool _cancelRequested;

    /// <summary>
    /// Признак того, что событие завершения уже отправлено подписчикам.
    /// </summary>
    private bool _completionRaised;

    /// <summary>
    /// Признак того, что событие отмены уже отправлено подписчикам.
    /// </summary>
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
    /// <param name="parTitle">Заголовок окна.</param>
    /// <param name="parCaption">Подпись, отображаемая внутри формы.</param>
    /// <param name="parMaximum">Максимальное количество обрабатываемых записей.</param>
    /// <param name="parCompletionMode">Определяет поведение после достижения максимального значения.</param>
    public void Initialize(string parTitle, string parCaption, double parMaximum, ProgressCompletionMode parCompletionMode)
    {
        if (parMaximum <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(parMaximum));
        }

        _completionMode = parCompletionMode;
        _maximum = parMaximum;
        _currentValue = 0;
        _cancelRequested = false;
        _completionRaised = false;
        _cancelRaised = false;

        Dispatcher.UIThread.Post(() =>
        {
            _viewModel = new ProgressWindowViewModel
            {
                Caption = parCaption,
                Details = FormatDetails(0, parMaximum),
                ProgressMaximum = parMaximum,
                ProgressValue = 0,
                CancelButtonText = "Отмена",
                IsCancelEnabled = true
            };

            _viewModel.CancellationRequested += OnCancellationRequested;

            _window = new ProgressWindow
            {
                DataContext = _viewModel,
                Title = parTitle,
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
    /// <param name="parProcessedCount">Количество обработанных записей. По умолчанию — 1.</param>
    /// <param name="parNewCaption">Необязательная новая подпись.</param>
    /// <returns><c>true</c>, если вызывающему коду следует прекратить обработку; иначе <c>false</c>.</returns>
    public bool ReportProgress(double parProcessedCount = 1, string? parNewCaption = null)
    {
        if (parProcessedCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(parProcessedCount));
        }

        lock (_syncRoot)
        {
            _currentValue = Math.Min(_currentValue + parProcessedCount, _maximum);
        }

        Dispatcher.UIThread.Post(() =>
        {
            if (_viewModel is null)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(parNewCaption))
            {
                _viewModel.Caption = parNewCaption;
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

    /// <summary>
    /// Реагирует на достижение максимального значения прогресса.
    /// </summary>
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

    /// <summary>
    /// Обрабатывает запрос отмены, поступивший из модели представления.
    /// </summary>
    /// <param name="parSender">Источник события.</param>
    /// <param name="parEventArgs">Аргументы события.</param>
    private void OnCancellationRequested(object? parSender, EventArgs parEventArgs)
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

    /// <summary>
    /// Обрабатывает попытку закрытия окна прогресса.
    /// </summary>
    /// <param name="parSender">Источник события.</param>
    /// <param name="parEventArgs">Аргументы события.</param>
    private void OnWindowClosing(object? parSender, WindowClosingEventArgs parEventArgs)
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

    /// <summary>
    /// Обрабатывает окончательное закрытие окна и освобождает ресурсы.
    /// </summary>
    /// <param name="parSender">Источник события.</param>
    /// <param name="parEventArgs">Аргументы события.</param>
    private void OnWindowClosed(object? parSender, EventArgs parEventArgs)
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

    /// <summary>
    /// Генерирует событие завершения формы при достижении максимума.
    /// </summary>
    private void RaiseCompletionEvent()
    {
        if (_completionRaised)
        {
            return;
        }

        _completionRaised = true;
        FormClosed?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Генерирует событие отмены, уведомляя подписчиков.
    /// </summary>
    private void RaiseCancellation()
    {
        if (_cancelRaised)
        {
            return;
        }

        _cancelRaised = true;
        OperationCancelled?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Формирует текстовое представление текущего прогресса.
    /// </summary>
    /// <param name="parCurrent">Текущее значение прогресса.</param>
    /// <param name="parMaximum">Максимальное значение прогресса.</param>
    /// <returns>Текст с информацией о ходе обработки.</returns>
    private static string FormatDetails(double parCurrent, double parMaximum)
    {
        return $"Обработано {parCurrent:N0} из {parMaximum:N0}";
    }
}
