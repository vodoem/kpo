using System.ComponentModel;
using System.Runtime.CompilerServices;

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

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

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
        set => SetField(ref _isCancelEnabled, value);
    }

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (Equals(field, value))
        {
            return;
        }

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
