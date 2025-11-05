namespace Duz_vadim_project.DesignPatterns.PrototypeFactory;

/// <summary>
/// Фабрика прототипов для создания объектов рыб через клонирование
/// </summary>
public class PrototypeFishFactory : IFishFactory
{
  private readonly FreshwaterFish _freshwaterPrototype;
  private readonly SaltwaterFish _saltwaterPrototype;

  /// <summary>
  /// Конструктор фабрики прототипов
  /// </summary>
  /// <param name="freshwaterPrototype">Прототип пресноводной рыбы</param>
  /// <param name="saltwaterPrototype">Прототип морской рыбы</param>
  public PrototypeFishFactory(FreshwaterFish freshwaterPrototype, SaltwaterFish saltwaterPrototype)
  {
    _freshwaterPrototype = freshwaterPrototype;
    _saltwaterPrototype = saltwaterPrototype;
  }

  /// <summary>
  /// Создает объект пресноводной рыбы через клонирование
  /// </summary>
  /// <returns>Клонированный экземпляр пресноводной рыбы</returns>
  public FreshwaterFish CreateFreshwaterFish()
  {
    return (FreshwaterFish)_freshwaterPrototype.Clone();
  }

  /// <summary>
  /// Создает объект морской рыбы через клонирование
  /// </summary>
  /// <returns>Клонированный экземпляр морской рыбы</returns>
  public SaltwaterFish CreateSaltwaterFish()
  {
    return (SaltwaterFish)_saltwaterPrototype.Clone();
  }
}