namespace Server;

/// <summary>
/// Ошибка при чтении повреждённого файла данных.
/// </summary>
public class DataFileCorruptedException : Exception
{
  /// <summary>
  /// Создаёт новое исключение с сообщением.
  /// </summary>
  public DataFileCorruptedException(string message, Exception? innerException = null)
    : base(message, innerException)
  {
  }
}
