using CommunityToolkit.Mvvm.ComponentModel;

namespace Duz_vadim_project;

/// <summary>
/// Карп
/// </summary>
public partial class Carp : FreshwaterFish
{
  /// <summary>
  /// Цвет чешуи
  /// </summary>
  [ObservableProperty]
  private string _scaleColor;

  /// <summary>
  /// Конструктор без параметров
  /// </summary>
  public Carp() : base()
  {
    _scaleColor = "Не указан";
  }

  /// <summary>
  /// Конструктор с параметрами
  /// </summary>
  /// <param name="parWeight">Вес рыбы</param>
  /// <param name="parAge">Возраст рыбы</param>
  /// <param name="parIsEdible">Является ли рыба съедобной</param>
  /// <param name="parHabitatDepth">Глубина обитания</param>
  /// <param name="parScaleColor">Цвет чешуи</param>
  public Carp(decimal parWeight, int parAge, bool parIsEdible, decimal parHabitatDepth, string parScaleColor) : base(parWeight, parAge, parIsEdible, parHabitatDepth)
  {
    _scaleColor = parScaleColor;
  }

  /// <summary>
  /// Метод для определения цвета чешуи
  /// </summary>
  /// <param name="parLightIntensity">Интенсивность света</param>
  public void DetermineScaleColor(int parLightIntensity)
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

    if (parOther is Carp carp)
    {
      ScaleColor = carp.ScaleColor;
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
  public Carp(Carp parOther) : base(parOther)
  {
    _scaleColor = parOther._scaleColor;
  }

  /// <summary>
  /// Создание копии объекта
  /// </summary>
  /// <returns>Клонированный объект</returns>
  public override object Clone()
  {
    return new Carp(this);
  }
}