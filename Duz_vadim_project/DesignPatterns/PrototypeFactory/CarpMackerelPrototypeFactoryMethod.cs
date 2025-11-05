using Duz_vadim_project.DesignPatterns.FactoryMethod;

namespace Duz_vadim_project.DesignPatterns.PrototypeFactory;

/// <summary>
/// Фабричный метод для создания фабрики прототипов карпа и скумбрии
/// </summary>
public class CarpMackerelPrototypeFactoryMethod : FishFactoryMethod
{
  /// <summary>
  /// Создает фабрику прототипов для карпа и скумбрии
  /// </summary>
  /// <returns>Фабрика прототипов</returns>
  public override IFishFactory CreateFactory()
  {
    var freshwaterPrototype = GetFreshwaterPrototype();
    var saltwaterPrototype = GetSaltwaterPrototype();

    return new PrototypeFishFactory(freshwaterPrototype, saltwaterPrototype);
  }

  /// <summary>
  /// Получает прототип пресноводной рыбы (карп)
  /// </summary>
  /// <returns>Прототип карпа</returns>
  private FreshwaterFish GetFreshwaterPrototype()
  {
    // Логика инициализации эталонного объекта пресноводной рыбы
    var prototype = new Carp();
    // Открытие формы редактирования для настройки эталонного объекта
    return prototype;
  }

  /// <summary>
  /// Получает прототип морской рыбы (скумбрия)
  /// </summary>
  /// <returns>Прототип скумбрии</returns>
  private SaltwaterFish GetSaltwaterPrototype()
  {
    var prototype = new Mackerel();
    return prototype;
  }
}
