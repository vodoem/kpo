namespace ProgressDisplay;

/// <summary>
/// Определяет поведение формы прогресса после достижения максимального количества записей.
/// </summary>
public enum ProgressCompletionMode
{
    /// <summary>
    /// Форма закрывается автоматически, когда прогресс достигает максимума.
    /// </summary>
    AutoClose,

    /// <summary>
    /// Форма остаётся открытой и ожидает, пока пользователь закроет её вручную.
    /// </summary>
    WaitForUserAction
}
