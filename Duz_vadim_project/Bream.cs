using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Duz_vadim_project;

/// <summary>
/// Лещ
/// </summary>
public partial class Bream : FreshwaterFish
{
  /// <summary>
  /// Форма тела
  /// </summary>
  [Required(ErrorMessage = "Форма тела обязательна")]
  [StringLength(100, ErrorMessage = "Слишком длинное описание формы")]
  [ObservableProperty]
  private string _bodyShape;

  /// <summary>
  /// Конструктор без параметров
  /// </summary>
  public Bream() : base()
  {
    _bodyShape = "Не указан";
  }

  /// <summary>
  /// Конструктор с параметрами
  /// </summary>
  /// <param name="parWeight">Вес рыбы</param>
  /// <param name="parAge">Возраст рыбы</param>
  /// <param name="parIsEdible">Является ли рыба съедобной</param>
  /// <param name="parHabitatDepth">Глубина обитания</param>
  /// <param name="parBodyShape">Форма тела</param>
  public Bream(decimal parWeight, int parAge, bool parIsEdible, decimal parHabitatDepth, string parBodyShape) : base(parWeight, parAge, parIsEdible, parHabitatDepth)
  {
    _bodyShape = parBodyShape;
  }

  /// <summary>
  /// Метод для определения формы тела
  /// </summary>
  /// <param name="parEnvironment">Окружающая среда</param>
  public void DetermineBodyShape(string parEnvironment)
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

    if (parOther is Bream bream)
    {
      BodyShape = bream.BodyShape;
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
  public Bream(Bream parOther) : base(parOther)
  {
    _bodyShape = parOther._bodyShape;
  }

  /// <summary>
  /// Создание копии объекта
  /// </summary>
  /// <returns>Клонированный объект</returns>
  public override object Clone()
  {
    return new Bream(this);
  }
}
