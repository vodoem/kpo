using Avalonia;
using System;
using Avalonia.ReactiveUI;

namespace UI;

/// <summary>
/// Главный класс программы, запускающий приложение Avalonia.
/// </summary>
sealed class Program
{
  /// <summary>
  /// Точка входа в приложение. Вызывается при запуске приложения.
  /// </summary>
  /// <param name="parArgs">Аргументы командной строки.</param>
  [STAThread]
  public static void Main(string[] parArgs) => BuildAvaloniaApp()
    .StartWithClassicDesktopLifetime(parArgs);

  /// <summary>
  /// Конфигурация Avalonia-приложения.
  /// </summary>
  /// <returns>Настроенный AppBuilder для приложения.</returns>
  private static AppBuilder BuildAvaloniaApp()
    => AppBuilder.Configure<App>()
      .UsePlatformDetect()
      .WithInterFont()
      .LogToTrace()
      .UseReactiveUI();
}