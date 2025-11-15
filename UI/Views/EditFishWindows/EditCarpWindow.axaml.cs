
using System;
using Avalonia.Controls;
using Avalonia.ReactiveUI;
using Duz_vadim_project;
using ReactiveUI;
using UI.ViewModels.EditFish;

namespace UI.Views.EditFishWindows;

/// <summary>
/// Окно для редактирования карпа.
/// </summary>
public partial class EditCarpWindow : ReactiveWindow<FishEditor<Carp>>
{
    /// <summary>
    /// Конструктор окна редактирования карпа.
    /// </summary>
    public EditCarpWindow()
    {
        InitializeComponent();

        // Подписываемся на команду сохранения.
        this.WhenActivated(disposables =>
        {
            disposables(ViewModel!.SaveChanges.Subscribe(Close));
        });

        // Обрабатываем нажатие кнопки «Отмена».
        this.FindControl<Button>("CancelButton")!.Click += (_, _) => Close(null);
    }
}