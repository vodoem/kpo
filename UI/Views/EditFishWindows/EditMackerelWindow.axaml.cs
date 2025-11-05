using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Duz_vadim_project;
using ReactiveUI;
using UI.ViewModels.EditFish;

namespace UI.Views.EditFishWindows;

/// <summary>
/// Окно для редактирования скумбрии.
/// </summary>
public partial class EditMackerelWindow : ReactiveWindow<FishEditor<Mackerel>>
{
  /// <summary>
  /// Конструктор окна редактирования скумбрии.
  /// </summary>
  public EditMackerelWindow()
  {
    InitializeComponent();

    // Подписываемся на команду сохранения
    this.WhenActivated(disposables =>
    {
      disposables(ViewModel!.SaveChanges.Subscribe(Close));
    });

    // Обработка кнопки "Отмена"
    this.FindControl<Button>("CancelButton")!.Click += (_, _) => Close(null);
  }
}