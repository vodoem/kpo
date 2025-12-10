using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.ReactiveUI;
using Avalonia.Threading;
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

    this.WhenActivated(disposables =>
    {
      ViewModel?.ShowEditFishDialog.RegisterHandler(DoShowDialogAsync).DisposeWith(disposables);
      ViewModel?.FocusFishRequest.RegisterHandler(FocusSelectedFishAsync).DisposeWith(disposables);
    });
  }

  /// <summary>
  /// Асинхронно отображает окно редактирования рыбы в зависимости от её типа.
  /// </summary>
  /// <param name="interaction">Контекст взаимодействия для отображения окна редактирования.</param>
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
  /// Обеспечивает перевод фокуса на выбранную запись после фильтрации.
  /// </summary>
  /// <param name="interaction">Контекст взаимодействия с информацией о выбранной рыбе.</param>
  private async Task FocusSelectedFishAsync(IInteractionContext<Fish, Unit> interaction)
  {
    await Dispatcher.UIThread.InvokeAsync(
      () =>
      {
        if (FishGrid != null)
        {
          if (!ReferenceEquals(FishGrid.SelectedItem, interaction.Input))
          {
            FishGrid.SelectedItem = null;
            FishGrid.SelectedItem = interaction.Input;
          }

          var firstColumn = FishGrid.Columns.Count > 0 ? FishGrid.Columns[0] : null;
          FishGrid.ScrollIntoView(interaction.Input, firstColumn);
          FishGrid.Focus();
        }

        interaction.SetOutput(Unit.Default);
      },
      DispatcherPriority.Render);
  }

  /// <summary>
  /// Обрабатывает изменение выбранной фабрики в выпадающем списке.
  /// </summary>
  /// <param name="sender">Источник события.</param>
  /// <param name="eventArgs">Аргументы события.</param>
  private void OnFactorySelectorChanged(object sender, SelectionChangedEventArgs eventArgs)
  {
    if (sender is ComboBox comboBox && ViewModel != null && comboBox.SelectedIndex >= 0)
    {
      ViewModel.InitializeFactories(comboBox.SelectedIndex);
    }
  }
}
