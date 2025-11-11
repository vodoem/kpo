using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace ProgressDisplay;

/// <summary>
/// Модель представления окна прогресса, предоставляющая состояние пользовательскому интерфейсу.
/// </summary>
public sealed class ProgressWindowViewModel : INotifyPropertyChanged
{
    /// <summary>
    /// Текст, отображаемый над индикатором прогресса.
    /// </summary>
    private string _caption = string.Empty;

    /// <summary>
    /// Дополнительное описание текущей операции.
    /// </summary>
    private string _details = string.Empty;

    /// <summary>
    /// Максимальное значение шкалы прогресса.
    /// </summary>
    private double _progressMaximum;

    /// <summary>
    /// Текущее значение шкалы прогресса.
    /// </summary>
    private double _progressValue;

    /// <summary>
    /// Надпись на кнопке отмены.
    /// </summary>
    private string _cancelButtonText = "Отмена";

    /// <summary>
    /// Признак доступности кнопки отмены.
    /// </summary>
    private bool _isCancelEnabled = true;

    /// <summary>
    /// Команда, вызываемая при нажатии на кнопку отмены.
    /// </summary>
    private readonly RelayCommand _cancelCommand;

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Событие запроса отмены длительной операции.
    /// </summary>
    public event EventHandler? CancellationRequested;

    /// <summary>
    /// Создаёт модель представления и настраивает команду отмены.
    /// </summary>
    public ProgressWindowViewModel()
    {
        _cancelCommand = new RelayCommand(_ => RaiseCancellationRequested(), _ => IsCancelEnabled);
    }

    /// <summary>
    /// Заголовок, отображаемый над индикатором выполнения.
    /// </summary>
    public string Caption
    {
        get => _caption;
        set => SetField(ref _caption, value);
    }

    /// <summary>
    /// Дополнительная подпись под индикатором выполнения.
    /// </summary>
    public string Details
    {
        get => _details;
        set => SetField(ref _details, value);
    }

    /// <summary>
    /// Максимальное значение индикатора выполнения.
    /// </summary>
    public double ProgressMaximum
    {
        get => _progressMaximum;
        set => SetField(ref _progressMaximum, value);
    }

    /// <summary>
    /// Текущее значение индикатора выполнения.
    /// </summary>
    public double ProgressValue
    {
        get => _progressValue;
        set => SetField(ref _progressValue, value);
    }

    /// <summary>
    /// Текст, отображаемый на кнопке отмены.
    /// </summary>
    public string CancelButtonText
    {
        get => _cancelButtonText;
        set => SetField(ref _cancelButtonText, value);
    }

    /// <summary>
    /// Признак доступности кнопки отмены.
    /// </summary>
    public bool IsCancelEnabled
    {
        get => _isCancelEnabled;
        set
        {
            if (SetField(ref _isCancelEnabled, value))
            {
                _cancelCommand.RaiseCanExecuteChanged();
            }
        }
    }

    /// <summary>
    /// Команда, инициирующая отмену операции пользователем.
    /// </summary>
    public ICommand CancelCommand => _cancelCommand;

    /// <summary>
    /// Генерирует событие запроса отмены.
    /// </summary>
    private void RaiseCancellationRequested()
    {
        CancellationRequested?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Устанавливает новое значение свойства и уведомляет об изменении.
    /// </summary>
    /// <typeparam name="T">Тип обновляемого поля.</typeparam>
    /// <param name="parField">Ссылка на поле, содержащее значение свойства.</param>
    /// <param name="parValue">Новое значение.</param>
    /// <param name="parPropertyName">Имя свойства, которое необходимо уведомить об изменении.</param>
    /// <returns><c>true</c>, если значение изменилось.</returns>
    private bool SetField<T>(ref T parField, T parValue, [CallerMemberName] string? parPropertyName = null)
    {
        if (Equals(parField, parValue))
        {
            return false;
        }

        parField = parValue;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(parPropertyName));
        return true;
    }

    /// <summary>
    /// Реализует простую команду для обработки нажатия кнопки.
    /// </summary>
    private sealed class RelayCommand : ICommand
    {
        /// <summary>
        /// Делегат выполнения команды.
        /// </summary>
        private readonly Action<object?> _execute;

        /// <summary>
        /// Делегат проверки возможности выполнения команды.
        /// </summary>
        private readonly Func<object?, bool> _canExecute;

        /// <summary>
        /// Создаёт новый экземпляр команды.
        /// </summary>
        /// <param name="parExecute">Делегат выполнения.</param>
        /// <param name="parCanExecute">Делегат проверки доступности.</param>
        public RelayCommand(Action<object?> parExecute, Func<object?, bool> parCanExecute)
        {
            _execute = parExecute ?? throw new ArgumentNullException(nameof(parExecute));
            _canExecute = parCanExecute ?? throw new ArgumentNullException(nameof(parCanExecute));
        }

        public event EventHandler? CanExecuteChanged;

        /// <summary>
        /// Определяет, доступна ли команда для выполнения.
        /// </summary>
        /// <param name="parParameter">Параметр команды.</param>
        /// <returns><c>true</c>, если команду можно выполнить.</returns>
        public bool CanExecute(object? parParameter)
        {
            return _canExecute(parParameter);
        }

        /// <summary>
        /// Выполняет команду.
        /// </summary>
        /// <param name="parParameter">Параметр команды.</param>
        public void Execute(object? parParameter)
        {
            _execute(parParameter);
        }

        /// <summary>
        /// Уведомляет подписчиков об изменении доступности команды.
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
