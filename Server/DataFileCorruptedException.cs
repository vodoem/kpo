namespace Server;

public sealed class DataFileCorruptedException : Exception
{
  public DataFileCorruptedException(string message, Exception? innerException = null)
    : base(message, innerException)
  {
  }
}
