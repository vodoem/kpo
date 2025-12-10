using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Duz_vadim_project;

/// <summary>
/// Пресноводные рыбы
/// </summary>
public partial class FreshwaterFish : Fish
{
  /// <summary>
  /// Глубина обитания в метрах
  /// </summary>
  [Range(0, 10_000, ErrorMessage = "Глубина обитания должна быть неотрицательной")]
  [ObservableProperty]
  private decimal _habitatDepth;

  /// <summary>
  /// Конструктор без параметров
  /// </summary>
  public FreshwaterFish() : base()
  {
    _habitatDepth = 0.0m;
  }

  /// <summary>
  /// Конструктор с параметрами
  /// </summary>
  /// <param name="parWeight">Вес рыбы</param>
  /// <param name="parAge">Возраст рыбы</param>
  /// <param name="parIsEdible">Является ли рыба съедобной</param>
  /// <param name="parHabitatDepth">Глубина обитания</param>
  public FreshwaterFish(decimal parWeight, int parAge, bool parIsEdible, decimal parHabitatDepth) : base(parWeight, parAge, parIsEdible)
  {
    _habitatDepth = parHabitatDepth;
  }

  /// <summary>
  /// Метод для определения среды обитания
  /// </summary>
  /// <param name="parTemperature">Температура воды</param>
  public void DetermineHabitat(decimal parTemperature)
  {
    // Реализация метода
  }

  /// <summary>
  /// Копирование данных из другого объекта
  /// </summary>
  /// <param name="parOther">Объект для копирования</param>
  public override void CopyFrom(Fish parOther)
  {
    base.CopyFrom(parOther);

    if (parOther is FreshwaterFish freshwater)
    {
      HabitatDepth = freshwater.HabitatDepth;
    }
    else
    {
      throw new ArgumentException("Неверный тип объекта для копирования", nameof(parOther));
    }
  }

  /// <summary>
  /// Конструктор копирования
  /// </summary>
  /// <param name="parOther">Копируемый объект</param>
  public FreshwaterFish(FreshwaterFish parOther) : base(parOther)
  {
    _habitatDepth = parOther._habitatDepth;
  }

  /// <summary>
  /// Создание копии объекта
  /// </summary>
  /// <returns>Клонированный объект</returns>
  public override object Clone()
  {
    return new FreshwaterFish(this);
  }
}
