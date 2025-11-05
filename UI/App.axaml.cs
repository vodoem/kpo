using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Markup.Xaml;
using UI.ViewModels;
using UI.Views;

namespace UI;


/// <summary>
/// Основной класс приложения. Инициализирует приложение и управляет жизненным циклом.
/// </summary>
public partial class App : Application
{
  /// <summary>
  /// Инициализирует компоненты приложения.
  /// </summary>
  public override void Initialize()
  {
    AvaloniaXamlLoader.Load(this);
  }

  /// <summary>
  /// Вызывается после завершения инициализации фреймворка.
  /// Настроивает главное окно приложения и его контекст данных.
  /// </summary>
  public override void OnFrameworkInitializationCompleted()
  {
    if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
    {
      desktop.MainWindow = new MainWindow
      {
        DataContext = new MainWindowViewModel() // Установка ViewModel
      };
    }

    base.OnFrameworkInitializationCompleted();
  }
}
