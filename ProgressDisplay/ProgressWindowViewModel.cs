using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ProgressDisplay;

/// <summary>
/// View model for the progress window that exposes state to the UI.
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
    /// Gets or sets the caption displayed above the progress bar.
    /// </summary>
    public string Caption
    {
        get => _caption;
        set => SetField(ref _caption, value);
    }

    /// <summary>
    /// Gets or sets additional details displayed below the progress bar.
    /// </summary>
    public string Details
    {
        get => _details;
        set => SetField(ref _details, value);
    }

    /// <summary>
    /// Gets or sets the maximum value of the progress bar.
    /// </summary>
    public double ProgressMaximum
    {
        get => _progressMaximum;
        set => SetField(ref _progressMaximum, value);
    }

    /// <summary>
    /// Gets or sets the current value of the progress bar.
    /// </summary>
    public double ProgressValue
    {
        get => _progressValue;
        set => SetField(ref _progressValue, value);
    }

    /// <summary>
    /// Gets or sets the text displayed on the cancel button.
    /// </summary>
    public string CancelButtonText
    {
        get => _cancelButtonText;
        set => SetField(ref _cancelButtonText, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the cancel button is enabled.
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
