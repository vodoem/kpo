using CommunityToolkit.Mvvm.ComponentModel;

namespace Duz_vadim_project;

/// <summary>
/// Морские рыбы
/// </summary>
public partial class SaltwaterFish : Fish
{
  /// <summary>
  /// Соленость воды в промилле
  /// </summary>
  [ObservableProperty]
  private decimal _salinity;

  /// <summary>
  /// Конструктор без параметров
  /// </summary>
  public SaltwaterFish() : base()
  {
    _salinity = 0.0m;
  }

  /// <summary>
  /// Конструктор с параметрами
  /// </summary>
  /// <param name="parWeight">Вес рыбы</param>
  /// <param name="parAge">Возраст рыбы</param>
  /// <param name="parIsEdible">Является ли рыба съедобной</param>
  /// <param name="parSalinity">Соленость воды</param>
  public SaltwaterFish(decimal parWeight, int parAge, bool parIsEdible, decimal parSalinity) : base(parWeight, parAge, parIsEdible)
  {
    _salinity = parSalinity;
  }

  /// <summary>
  /// Метод для определения уровня солености
  /// </summary>
  /// <param name="parWaterVolume">Объем воды</param>
  public void DetermineSalinityLevel(decimal parWaterVolume)
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

    if (parOther is SaltwaterFish saltwater)
    {
      Salinity = saltwater.Salinity;
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
  public SaltwaterFish(SaltwaterFish parOther) : base(parOther)
  {
    _salinity = parOther._salinity;
  }

  /// <summary>
  /// Создание копии объекта
  /// </summary>
  /// <returns>Клонированный объект</returns>
  public override object Clone()
  {
    return new SaltwaterFish(this);
  }
}