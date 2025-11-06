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
    private string _caption = string.Empty;
    private string _details = string.Empty;
    private double _progressMaximum;
    private double _progressValue;
    private string _cancelButtonText = "Отмена";
    private bool _isCancelEnabled = true;
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

    private void RaiseCancellationRequested()
    {
        CancellationRequested?.Invoke(this, EventArgs.Empty);
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (Equals(field, value))
        {
            return false;
        }

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        return true;
    }

    private sealed class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Func<object?, bool> _canExecute;

        public RelayCommand(Action<object?> execute, Func<object?, bool> canExecute)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute ?? throw new ArgumentNullException(nameof(canExecute));
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
        {
            return _canExecute(parameter);
        }

        public void Execute(object? parameter)
        {
            _execute(parameter);
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
