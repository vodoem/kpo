namespace ProgressDisplay;

/// <summary>
/// Описывает параметры инициализации окна прогресса.
/// </summary>
public sealed class ProgressExecutionOptions
{
  /// <summary>
  /// Создаёт экземпляр параметров инициализации.
  /// </summary>
  /// <param name="parTitle">Заголовок окна.</param>
  /// <param name="parCaption">Подпись внутри окна.</param>
  /// <param name="parMaximum">Максимальное количество обрабатываемых элементов.</param>
  /// <param name="parCompletionMode">Режим завершения работы формы.</param>
  public ProgressExecutionOptions(string parTitle, string parCaption, double parMaximum, ProgressCompletionMode parCompletionMode)
  {
    Title = parTitle;
    Caption = parCaption;
    Maximum = parMaximum;
    CompletionMode = parCompletionMode;
  }

  /// <summary>
  /// Заголовок окна прогресса.
  /// </summary>
  public string Title { get; }

  /// <summary>
  /// Подпись, отображаемая над индикатором выполнения.
  /// </summary>
  public string Caption { get; }

  /// <summary>
  /// Максимальное значение индикатора прогресса.
  /// </summary>
  public double Maximum { get; }

  /// <summary>
  /// Режим завершения после достижения максимального значения.
  /// </summary>
  public ProgressCompletionMode CompletionMode { get; }
}
