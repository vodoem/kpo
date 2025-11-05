using System;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.ReactiveUI;
using Duz_vadim_project;
using ReactiveUI;
using UI.ViewModels;
using UI.ViewModels.EditFish;
using UI.Views.EditFishWindows;


namespace UI.Views;

/// <summary>
/// Главное окно приложения для работы с рыбами.
/// </summary>
public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{
  /// <summary>
  /// Конструктор главного окна.
  /// </summary>
  public MainWindow()
  {
    InitializeComponent();
    ViewModel = new MainWindowViewModel();

    this.WhenActivated(disposables =>
    {
      ViewModel.ShowEditFishDialog.RegisterHandler(DoShowDialogAsync).DisposeWith(disposables);
    });
  }

  /// <summary>
  /// Асинхронно отображает окно редактирования рыбы в зависимости от типа рыбы.
  /// </summary>
  /// <param name="interaction">Контекст взаимодействия для отображения окна редактирования.</param>
  /// <returns>Задача, которая завершится, когда окно будет закрыто.</returns>
  private async Task DoShowDialogAsync(IInteractionContext<ViewModelBase, Fish?> interaction)
  {
    Window? dialog = interaction.Input switch
    {
      FishEditor<Bream> vm => new EditBreamWindow { DataContext = vm },
      FishEditor<Carp> vm => new EditCarpWindow { DataContext = vm },
      FishEditor<Mackerel> vm => new EditMackerelWindow { DataContext = vm },
      FishEditor<Tuna> vm => new EditTunaWindow { DataContext = vm },
      _ => null
    };

    if (dialog is null)
    {
      interaction.SetOutput(null);
      return;
    }

    var result = await dialog.ShowDialog<Fish?>(this);
    interaction.SetOutput(result);
  }
  
  /// <summary>
  /// Обработчик события изменения выбора фабрики в выпадающем списке
  /// </summary>
  /// <param name="parSender">Источник события</param>
  /// <param name="parEventArgs">Аргументы события</param>
  private void OnFactorySelectorChanged(object parSender, SelectionChangedEventArgs parEventArgs)
  {
    if (parSender is ComboBox comboBox && ViewModel != null && comboBox.SelectedIndex >= 0)
    {
      ViewModel.InitializeFactories(comboBox.SelectedIndex);
    }
  }
}