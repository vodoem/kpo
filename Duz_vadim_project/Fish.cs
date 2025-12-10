using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Duz_vadim_project;

/// <summary>
/// Все виды рыб
/// </summary>
public partial class Fish : ObservableValidator, ICloneable
{
  /// <summary>
  /// Уникальный идентификатор, выдаваемый сервером.
  /// </summary>
  [JsonInclude]
  [Range(0, int.MaxValue, ErrorMessage = "Идентификатор должен быть неотрицательным")]
  public int Id { get; private set; }

  /// <summary>
  /// Вес рыбы в граммах
  /// </summary>
  [Range(0, 1_000_000, ErrorMessage = "Вес не может быть отрицательным")]
  [ObservableProperty]
  private decimal _weight;

  /// <summary>
  /// Возраст рыбы в годах
  /// </summary>
  [Range(0, 200, ErrorMessage = "Возраст должен быть в разумных пределах")]
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
  [JsonInclude]
  public virtual string TypeName => GetType().Name;

  /// <summary>
  /// Устанавливает идентификатор, полученный от сервера.
  /// </summary>
  /// <param name="id">Присваиваемый идентификатор.</param>
  public void ApplyServerId(int id)
  {
    if (id < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(id), "Идентификатор не может быть отрицательным");
    }

    Id = id;
  }

  /// <summary>
  /// Конструктор без параметров
  /// </summary>
  public Fish()
  {
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
    Id = parParOther.Id;
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
