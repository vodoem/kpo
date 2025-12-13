using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json.Serialization;

namespace Duz_vadim_project;

/// <summary>
/// Все виды рыб
/// </summary>
public partial class Fish : ObservableObject, ICloneable
{
  /// <summary>
  /// Бэкинг-филд для идентификатора.
  /// </summary>
  private int _id;

  /// <summary>
  /// Уникальный идентификатор экземпляра. Значение назначается на сервере.
  /// </summary>
  [JsonInclude]
  public int Id
  {
    get => _id;
    private set => SetProperty(ref _id, value);
  }

  /// <summary>
  /// Вес рыбы в граммах
  /// </summary>
  [ObservableProperty]
  private decimal _weight;

  /// <summary>
  /// Возраст рыбы в годах
  /// </summary>
  [ObservableProperty]
  private int _age;

  /// <summary>
  /// Является ли рыба съедобной
  /// </summary>
  [ObservableProperty]
  private bool _isEdible;

  /// <summary>
  /// Строкове представление типа объекта
  /// </summary>
  public string TypeName => GetType().Name;

  /// <summary>
  /// Конструктор без параметров
  /// </summary>
  public Fish()
  {
    _id = 0;
    _weight = 0.0m;
    _age = 0;
    _isEdible = false;
  }

  /// <summary>
  /// Конструктор с параметрами
  /// </summary>
  /// <param name="parWeight">Вес рыбы</param>
  /// <param name="parAge">Возраст рыбы</param>
  /// <param name="parIsEdible">Является ли рыба съедобной</param>
  public Fish(decimal parWeight, int parAge, bool parIsEdible)
  {
    _id = 0;
    _weight = parWeight;
    _age = parAge;
    _isEdible = parIsEdible;
  }
  
  /// <summary>
  /// Копирование данных из другого объекта
  /// </summary>
  /// <param name="parOther">Объект для копирования</param>
  public virtual void CopyFrom(Fish parOther)
  {
    if (parOther == null)
    {
      throw new ArgumentNullException(nameof(parOther), "Объект для копирования не может быть null");
    }

    Id = parOther.Id;
    Weight = parOther.Weight;
    Age = parOther.Age;
    IsEdible = parOther.IsEdible;
  }

  /// <summary>
  /// Конструктор копирования
  /// </summary>
  /// <param name="parParOther">Копируемый объект</param>
  public Fish(Fish parParOther)
  {
    _id = parParOther._id;
    _weight = parParOther._weight;
    _age = parParOther._age;
    _isEdible = parParOther._isEdible;
  }

  /// <summary>
  /// Создание копии объекта
  /// </summary>
  /// <returns>Клонированный объект</returns>
  public virtual object Clone()
  {
    return new Fish(this);
  }
}