using Duz_vadim_project.DesignPatterns.AbstractFishFactory;

namespace Duz_vadim_project.DesignPatterns.FactoryMethod;

/// Фабричный метод для создания фабрики карпа и скумбрии
/// </summary>
public class CarpMackerelFactoryMethod : FishFactoryMethod
{
  /// <summary>
  /// Создает фабрику для карпа и скумбрии
  /// </summary>
  /// <returns>Фабрика</returns>
  public override IFishFactory CreateFactory()
  {
    return new CarpMackerelFactory();
  }
}