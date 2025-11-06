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
/// Основной класс приложения. Инициализирует приложение и управляет его жизненным циклом.
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
    /// Завершает настройку приложения, создавая главное окно и его модель представления.
    /// </summary>
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel() // Подключение модели представления главного окна.
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
