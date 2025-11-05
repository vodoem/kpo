using System;
using System.Reactive;
using Duz_vadim_project;
using ReactiveUI;

namespace UI.ViewModels.EditFish;

/// <summary>
/// ViewModel для редактирования любого типа рыбы
/// </summary>
/// <typeparam name="TFish">Тип рыбы</typeparam>
public partial class FishEditor<TFish> : ViewModelBase where TFish : Fish, ICloneable
{
  
  private readonly TFish _fishInstance;
  private readonly bool _isViewMode;
  
  /// <summary>
  /// Текущий экземпляр рыбы
  /// </summary>
  public TFish FishInstance => _fishInstance;
  
  /// <summary>
  /// Флаг, определяющий, доступна ли форма для редактирования
  /// </summary>
  public bool IsViewMode => _isViewMode;

  /// <summary>
  /// Заголовок окна
  /// </summary>
  public string WindowTitle => _isViewMode ? "Просмотр данных о рыбе" : $"Редактирование {FishInstance.GetType().Name}";
  
  /// <summary>
  /// Команда сохранения изменений
  /// </summary>
  public ReactiveCommand<Unit, TFish> SaveChanges { get; }
  
  /// <summary>
  /// Инициализация без параметров
  /// </summary>
  public FishEditor() : this(null, false) { }

  /// <summary>
  /// Инициализация с параметрами
  /// </summary>
  /// <param name="instance">Экземпляр рыбы</param>
  /// <param name="viewMode">Флаг режима просмотра</param>
  public FishEditor(TFish instance, bool viewMode)
  {
    _fishInstance = instance?.Clone() as TFish ?? Activator.CreateInstance<TFish>();
    _isViewMode = viewMode;

    SaveChanges = ReactiveCommand.Create(() => _fishInstance);
  }
}