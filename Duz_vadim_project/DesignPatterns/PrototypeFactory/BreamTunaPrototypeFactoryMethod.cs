using Duz_vadim_project.DesignPatterns.FactoryMethod;

namespace Duz_vadim_project.DesignPatterns.PrototypeFactory;

/// <summary>
/// Фабричный метод для создания фабрики прототипов леща и тунца
/// </summary>
public class BreamTunaPrototypeFactoryMethod : FishFactoryMethod
{
  /// <summary>
  /// Создает фабрику прототипов для леща и тунца
  /// </summary>
  /// <returns>Фабрика прототипов</returns>
  public override IFishFactory CreateFactory()
  {
    var freshwaterPrototype = GetFreshwaterPrototype();
    var saltwaterPrototype = GetSaltwaterPrototype();

    return new PrototypeFishFactory(freshwaterPrototype, saltwaterPrototype);
  }

  /// <summary>
  /// Получает прототип пресноводной рыбы (лещ)
  /// </summary>
  /// <returns>Прототип леща</returns>
  private FreshwaterFish GetFreshwaterPrototype()
  {
    // Логика инициализации эталонного объекта пресноводной рыбы
    var prototype = new Bream();
    // Открытие формы редактирования для настройки эталонного объекта
    return prototype;
  }

  /// <summary>
  /// Получает прототип морской рыбы (тунец)
  /// </summary>
  /// <returns>Прототип тунца</returns>
  private SaltwaterFish GetSaltwaterPrototype()
  {
    var prototype = new Tuna();
    return prototype;
  }
}
