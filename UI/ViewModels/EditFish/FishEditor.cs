using System;
using System.Reactive;
using Duz_vadim_project;
using ReactiveUI;

namespace UI.ViewModels.EditFish;

/// <summary>
/// Модель представления для редактирования любого вида рыбы.
/// </summary>
/// <typeparam name="TFish">Тип редактируемой рыбы.</typeparam>
public partial class FishEditor<TFish> : ViewModelBase where TFish : Fish, ICloneable
{
    private readonly TFish _fishInstance;
    private readonly bool _isViewMode;

    /// <summary>
    /// Текущий экземпляр рыбы.
    /// </summary>
    public TFish FishInstance => _fishInstance;

    /// <summary>
    /// Флаг, определяющий, доступна ли форма только для просмотра.
    /// </summary>
    public bool IsViewMode => _isViewMode;

    /// <summary>
    /// Заголовок окна.
    /// </summary>
    public string WindowTitle => _isViewMode ? "Просмотр данных о рыбе" : $"Редактирование {FishInstance.GetType().Name}";

    /// <summary>
    /// Команда сохранения изменений.
    /// </summary>
    public ReactiveCommand<Unit, TFish> SaveChanges { get; }

    /// <summary>
    /// Создаёт форму без предварительных данных.
    /// </summary>
    public FishEditor() : this(null, false)
    {
    }

    /// <summary>
    /// Создаёт форму с переданным экземпляром рыбы.
    /// </summary>
    /// <param name="instance">Экземпляр рыбы, который может быть равен null.</param>
    /// <param name="viewMode">Флаг режима просмотра.</param>
    public FishEditor(TFish? instance, bool viewMode)
    {
        _fishInstance = instance?.Clone() as TFish ?? Activator.CreateInstance<TFish>();
        _isViewMode = viewMode;

        SaveChanges = ReactiveCommand.Create(() => _fishInstance);
    }
}