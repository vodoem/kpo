namespace Duz_vadim_project.DesignPatterns.FactoryMethod;

/// <summary>
/// Абстрактный класс фабричного метода для создания фабрик
/// </summary>
public abstract class FishFactoryMethod
{
  /// <summary>
  /// Создает фабрику
  /// </summary>
  /// <returns>Фабрика</returns>
  public abstract IFishFactory CreateFactory();
}