using System;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace ProgressDisplay;

/// <summary>
/// Interaction logic for the reusable progress window.
/// </summary>
public partial class ProgressWindow : Window
{
    /// <summary>
    /// Occurs when the cancel button is clicked.
    /// </summary>
    public event EventHandler? CancelRequested;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProgressWindow"/> class.
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
