namespace ProgressDisplay;

/// <summary>
/// Defines how the progress form behaves once the maximum number of records is reached.
/// </summary>
public enum ProgressCompletionMode
{
    /// <summary>
    /// The form closes automatically when the progress reaches the maximum value.
    /// </summary>
    AutoClose,

    /// <summary>
    /// The form remains open and waits for the user to close it manually.
    /// </summary>
    WaitForUserAction
}
