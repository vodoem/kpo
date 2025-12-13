using System.Threading;

namespace ProgressDisplay;

/// <summary>
/// Описывает предоставляемый длительной операции интерфейс взаимодействия с окном прогресса.
/// </summary>
public interface IProgressSession
{
  /// <summary>
  /// Возвращает максимальное количество обрабатываемых элементов.
  /// </summary>
  double Maximum { get; }

  /// <summary>
  /// Возвращает токен отмены, который активируется при нажатии пользователем кнопки отмены
  /// либо закрытии окна прогресса.
  /// </summary>
  CancellationToken CancellationToken { get; }

  /// <summary>
  /// Передаёт сведения о продвижении операции и возвращает признак необходимости остановки.
  /// </summary>
  /// <param name="parProcessedCount">Количество обработанных записей. По умолчанию — 1.</param>
  /// <param name="parNewCaption">Необязательная новая подпись.</param>
  /// <returns><c>true</c>, если операция должна быть прервана.</returns>
  bool ReportProgress(double parProcessedCount = 1, string? parNewCaption = null);

  /// <summary>
  /// Принудительно закрывает окно прогресса.
  /// </summary>
  void Close();

  /// <summary>
  /// Возвращает признак отмены операции пользователем.
  /// </summary>
  bool IsCancellationRequested { get; }
}
