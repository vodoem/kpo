using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Duz_vadim_project;

/// <summary>
/// Тунец
/// </summary>
public partial class Tuna : SaltwaterFish
{
  /// <summary>
  /// Скорость плавания
  /// </summary>
  [Range(0, 200, ErrorMessage = "Скорость плавания должна быть неотрицательной")]
  [ObservableProperty]
  private decimal _swimmingSpeed;

  /// <summary>
  /// Конструктор без параметров
  /// </summary>
  public Tuna() : base()
  {
    _swimmingSpeed = 0.0m;
  }

  /// <summary>
  /// Конструктор с параметрами
  /// </summary>
  /// <param name="parWeight">Вес рыбы</param>
  /// <param name="parAge">Возраст рыбы</param>
  /// <param name="parIsEdible">Является ли рыба съедобной</param>
  /// <param name="parSalinity">Соленость воды</param>
  /// <param name="parSwimmingSpeed">Скорость плавания</param>
  public Tuna(decimal parWeight, int parAge, bool parIsEdible, decimal parSalinity, decimal parSwimmingSpeed) : base(parWeight, parAge, parIsEdible, parSalinity)
  {
    _swimmingSpeed = parSwimmingSpeed;
  }

  /// <summary>
  /// Метод для определения скорости плавания
  /// </summary>
  /// <param name="parWaterCurrent">Сила течения воды</param>
  public void DetermineSwimmingSpeed(decimal parWaterCurrent)
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

    if (parOther is Tuna tuna)
    {
      SwimmingSpeed = tuna.SwimmingSpeed;
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
  public Tuna(Tuna parOther) : base(parOther)
  {
    _swimmingSpeed = parOther._swimmingSpeed;
  }

  /// <summary>
  /// Создание копии объекта
  /// </summary>
  /// <returns>Клонированный объект</returns>
  public override object Clone()
  {
    return new Tuna(this);
  }
}
