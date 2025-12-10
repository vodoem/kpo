using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Duz_vadim_project;

/// <summary>
/// Скумбрия
/// </summary>
public partial class Mackerel : SaltwaterFish
{
  /// <summary>
  /// Размер жабр
  /// </summary>
  [Range(0, 500, ErrorMessage = "Размер жабр должен быть неотрицательным")]
  [ObservableProperty]
  private decimal _gillSize;

  /// <summary>
  /// Конструктор без параметров
  /// </summary>
  public Mackerel() : base()
  {
    _gillSize = 0.0m;
  }

  /// <summary>
  /// Конструктор с параметрами
  /// </summary>
  /// <param name="parWeight">Вес рыбы</param>
  /// <param name="parAge">Возраст рыбы</param>
  /// <param name="parIsEdible">Является ли рыба съедобной</param>
  /// <param name="parSalinity">Соленость воды</param>
  /// <param name="parGillSize">Размер жабр</param>
  public Mackerel(decimal parWeight, int parAge, bool parIsEdible, decimal parSalinity, decimal parGillSize) : base(parWeight, parAge, parIsEdible, parSalinity)
  {
    _gillSize = parGillSize;
  }

  /// <summary>
  /// Метод для определения размера жабр
  /// </summary>
  /// <param name="parWaterOxygenLevel">Уровень кислорода в воде</param>
  public void DetermineGillSize(decimal parWaterOxygenLevel)
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

    if (parOther is Mackerel mackerel)
    {
      GillSize = mackerel.GillSize;
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
  public Mackerel(Mackerel parOther) : base(parOther)
  {
    _gillSize = parOther._gillSize;
  }

  /// <summary>
  /// Создание копии объекта
  /// </summary>
  /// <returns>Клонированный объект</returns>
  public override object Clone()
  {
    return new Mackerel(this);
  }
}
