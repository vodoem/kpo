using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;

namespace ProgressDisplay;

/// <summary>
/// Управляет жизненным циклом окна прогресса и выполняемой длительной операцией.
/// </summary>
public sealed class ProgressFormController
{
  /// <summary>
  /// Объект синхронизации для обновления данных прогресса.
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
  /// Максимальное значение прогресса.
  /// </summary>
  private double _maximum;

  /// <summary>
  /// Текущее значение прогресса.
  /// </summary>
  private double _currentValue;

  /// <summary>
  /// Выбранный режим завершения работы формы.
  /// </summary>
  private ProgressCompletionMode _completionMode;

  /// <summary>
  /// Признак того, что пользователь запросил отмену операции.
  /// </summary>
  private bool _cancelRequested;

  /// <summary>
  /// Признак того, что событие завершения уже было отправлено подписчикам.
  /// </summary>
  private bool _completionRaised;

  /// <summary>
  /// Признак того, что событие отмены уже было отправлено подписчикам.
  /// </summary>
  private bool _cancelRaised;

  /// <summary>
  /// Источник токена отмены длительной операции.
  /// </summary>
  private CancellationTokenSource? _cancellationSource;

  /// <summary>
  /// Событие отмены операции пользователем или из-за ошибки.
  /// </summary>
  public event EventHandler? OperationCancelled;

  /// <summary>
  /// Событие успешного завершения операции.
  /// </summary>
  public event EventHandler? OperationCompleted;

  /// <summary>
  /// Асинхронно выполняет указанную операцию с отображением окна прогресса.
  /// </summary>
  /// <param name="parOptions">Параметры инициализации окна прогресса.</param>
  /// <param name="parOperation">Делегат, реализующий основную работу.</param>
  public async Task RunAsync(ProgressExecutionOptions parOptions, Func<IProgressSession, Task> parOperation)
  {
    if (parOptions is null)
    {
      throw new ArgumentNullException(nameof(parOptions));
    }

    if (parOperation is null)
    {
      throw new ArgumentNullException(nameof(parOperation));
    }

    await InitializeAsync(parOptions).ConfigureAwait(false);

    var session = new ProgressSession(this);

    try
    {
      await Task.Run(() => parOperation(session), session.CancellationToken).ConfigureAwait(false);
      if (!session.IsCancellationRequested)
      {
        await Dispatcher.UIThread.InvokeAsync(
          () => HandleCompletionReached(),
          DispatcherPriority.Background);
      }
    }
    catch (OperationCanceledException)
    {
      await Dispatcher.UIThread.InvokeAsync(
        () => HandleCancellationRequest(),
        DispatcherPriority.Background);
      throw;
    }
    catch (Exception ex)
    {
      await Dispatcher.UIThread.InvokeAsync(
        () => ReportFailure(ex.Message),
        DispatcherPriority.Background);
      throw;
    }
    finally
    {
      if (_completionMode == ProgressCompletionMode.AutoClose && _currentValue >= _maximum)
      {
        Close();
      }
    }
  }

  /// <summary>
  /// Асинхронно выполняет указанную операцию и возвращает её результат.
  /// </summary>
  /// <typeparam name="TResult">Тип возвращаемого значения.</typeparam>
  /// <param name="parOptions">Параметры инициализации окна прогресса.</param>
  /// <param name="parOperation">Делегат, реализующий основную работу.</param>
  /// <returns>Результат выполнения операции.</returns>
  public async Task<TResult> RunAsync<TResult>(ProgressExecutionOptions parOptions, Func<IProgressSession, Task<TResult>> parOperation)
  {
    if (parOperation is null)
    {
      throw new ArgumentNullException(nameof(parOperation));
    }

    TResult result = default!;

    await RunAsync(
      parOptions,
      async session =>
      {
        result = await parOperation(session).ConfigureAwait(false);
      }).ConfigureAwait(false);

    return result;
  }

  /// <summary>
  /// Инициирует закрытие окна прогресса.
  /// </summary>
  public void Close()
  {
    Dispatcher.UIThread.Post(
      () =>
      {
        if (_window != null)
        {
          _window.Close();
        }
      });
  }

  /// <summary>
  /// Возвращает признак того, что операция отменена пользователем.
  /// </summary>
  internal bool IsCancellationRequested => _cancelRequested;

  /// <summary>
  /// Возвращает текущий токен отмены.
  /// </summary>
  internal CancellationToken CancellationToken => _cancellationSource?.Token ?? CancellationToken.None;

  /// <summary>
  /// Внутренне обновляет прогресс и возвращает признак необходимости прерывания операции.
  /// </summary>
  /// <param name="parProcessedCount">Количество обработанных записей.</param>
  /// <param name="parNewCaption">Необязательная новая подпись.</param>
  /// <returns><c>true</c>, если операция должна быть остановлена.</returns>
  internal bool ReportProgressInternal(double parProcessedCount, string? parNewCaption)
  {
    if (parProcessedCount < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(parProcessedCount));
    }

    lock (_syncRoot)
    {
      _currentValue = Math.Min(_currentValue + parProcessedCount, _maximum);
    }

    Dispatcher.UIThread.Post(
      () =>
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
      },
      DispatcherPriority.Background);

    return _cancelRequested;
  }

  /// <summary>
  /// Асинхронно создаёт окно прогресса и отображает его пользователю.
  /// </summary>
  /// <param name="parOptions">Параметры инициализации.</param>
  private async Task InitializeAsync(ProgressExecutionOptions parOptions)
  {
    if (parOptions.Maximum <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(parOptions.Maximum));
    }

    _maximum = parOptions.Maximum;
    _currentValue = 0;
    _completionMode = parOptions.CompletionMode;
    _cancelRequested = false;
    _completionRaised = false;
    _cancelRaised = false;
    _cancellationSource = new CancellationTokenSource();

    await Dispatcher.UIThread.InvokeAsync(
      () =>
      {
        _viewModel = new ProgressWindowViewModel
        {
          Caption = parOptions.Caption,
          Details = FormatDetails(0, parOptions.Maximum),
          ProgressMaximum = parOptions.Maximum,
          ProgressValue = 0,
          CancelButtonText = "Отмена",
          IsCancelEnabled = true
        };

        _viewModel.CancellationRequested += OnCancellationRequested;

        _window = new ProgressWindow
        {
          DataContext = _viewModel,
          Title = parOptions.Title,
          CanResize = false,
          WindowStartupLocation = WindowStartupLocation.CenterOwner
        };

        _window.Closing += OnWindowClosing;
        _window.Closed += OnWindowClosed;

        var lifetime = Application.Current?.ApplicationLifetime;
        if (lifetime is IClassicDesktopStyleApplicationLifetime desktop && desktop.MainWindow != null)
        {
          _window.Show(desktop.MainWindow);
        }
        else
        {
          _window.Show();
        }
      });
  }

  /// <summary>
  /// Обрабатывает достижение максимального значения прогресса.
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
    }

    if (_currentValue >= _maximum)
    {
      RaiseCompletionEvent();
    }
  }

  /// <summary>
  /// Обрабатывает запрос отмены пользователем.
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
    HandleCancellationRequest();
  }

  /// <summary>
  /// Реагирует на попытку закрытия окна пользователем.
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
      HandleCancellationRequest();
    }
  }

  /// <summary>
  /// Завершает обработку после закрытия окна и освобождает ресурсы.
  /// </summary>
  /// <param name="parSender">Источник события.</param>
  /// <param name="parEventArgs">Аргументы события.</param>
  private void OnWindowClosed(object? parSender, EventArgs parEventArgs)
  {
    if (_viewModel != null)
    {
      _viewModel.IsCancelEnabled = false;
      _viewModel.CancellationRequested -= OnCancellationRequested;
    }

    if (_window != null)
    {
      _window.Closing -= OnWindowClosing;
      _window.Closed -= OnWindowClosed;
    }

    _window = null;
    _viewModel = null;
    _cancellationSource?.Dispose();
    _cancellationSource = null;
  }

  /// <summary>
  /// Сообщает операции об отмене и активирует токен отмены.
  /// </summary>
  private void HandleCancellationRequest()
  {
    if (_cancelRaised)
    {
      return;
    }

    _cancelRequested = true;
    _cancelRaised = true;
    _cancellationSource?.Cancel();
    OperationCancelled?.Invoke(this, EventArgs.Empty);
  }

  /// <summary>
  /// Генерирует событие завершения при достижении максимального значения.
  /// </summary>
  private void RaiseCompletionEvent()
  {
    if (_completionRaised)
    {
      return;
    }

    _completionRaised = true;
    OperationCompleted?.Invoke(this, EventArgs.Empty);
  }

  /// <summary>
  /// Отображает сообщение об ошибке в окне прогресса.
  /// </summary>
  /// <param name="parMessage">Текст ошибки.</param>
  private void ReportFailure(string parMessage)
  {
    if (_viewModel == null)
    {
      return;
    }

    _viewModel.Caption = "Произошла ошибка";
    _viewModel.Details = parMessage;
    _viewModel.CancelButtonText = "Закрыть";
    _viewModel.IsCancelEnabled = true;
    HandleCancellationRequest();
  }

  /// <summary>
  /// Формирует строку с информацией о ходе выполнения.
  /// </summary>
  /// <param name="parCurrent">Текущее значение.</param>
  /// <param name="parMaximum">Максимальное значение.</param>
  /// <returns>Строка состояния.</returns>
  private static string FormatDetails(double parCurrent, double parMaximum)
  {
    return $"Обработано {parCurrent:N0} из {parMaximum:N0}";
  }

  /// <summary>
  /// Реализация интерфейса взаимодействия с окном прогресса.
  /// </summary>
  private sealed class ProgressSession : IProgressSession
  {
    /// <summary>
    /// Контроллер, управляющий окном прогресса.
    /// </summary>
    private readonly ProgressFormController _controller;

    /// <summary>
    /// Создаёт новый экземпляр с привязкой к контроллеру.
    /// </summary>
    /// <param name="parController">Контроллер формы прогресса.</param>
    public ProgressSession(ProgressFormController parController)
    {
      _controller = parController;
    }

    /// <inheritdoc />
    public double Maximum => _controller._maximum;

    /// <inheritdoc />
    public CancellationToken CancellationToken => _controller.CancellationToken;

    /// <inheritdoc />
    public bool ReportProgress(double parProcessedCount = 1, string? parNewCaption = null)
    {
      return _controller.ReportProgressInternal(parProcessedCount, parNewCaption);
    }

    /// <inheritdoc />
    public void Close()
    {
      _controller.Close();
    }

    /// <inheritdoc />
    public bool IsCancellationRequested => _controller.IsCancellationRequested;
  }
}
