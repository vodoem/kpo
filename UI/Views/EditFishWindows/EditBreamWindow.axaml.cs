using System;
using Avalonia.Controls;
using Avalonia.ReactiveUI;
using Duz_vadim_project;
using ReactiveUI;
using UI.ViewModels.EditFish;

namespace UI.Views.EditFishWindows;


/// <summary>
/// Окно для редактирования леща.
/// </summary>
public partial class EditBreamWindow : ReactiveWindow<FishEditor<Bream>>
{
    /// <summary>
    /// Конструктор окна редактирования леща.
    /// </summary>
    public EditBreamWindow()
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