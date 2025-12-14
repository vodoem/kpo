using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Duz_vadim_project;
using ReactiveUI;
using UI.ViewModels.EditFish;
using System.Reactive.Linq;

namespace UI.Views.EditFishWindows;

/// <summary>
/// Окно для редактирования тунца.
/// </summary>
public partial class EditTunaWindow : ReactiveWindow<FishEditor<Tuna>>
{
    /// <summary>
    /// Конструктор окна редактирования тунца.
    /// </summary>
    public EditTunaWindow()
    {
        InitializeComponent();

        // Подписываемся на команду сохранения.
        this.WhenActivated(disposables =>
        {
            disposables(ViewModel!.SaveChanges
                .Where(result => result is not null)
                .Subscribe(Close));
        });

        // Обрабатываем нажатие кнопки «Отмена».
        this.FindControl<Button>("CancelButton")!.Click += (_, _) => Close(null);
    }
}