using Duz_vadim_project.DesignPatterns.AbstractFishFactory;

namespace Duz_vadim_project.DesignPatterns.FactoryMethod;

/// <summary>
/// Фабричный метод для создания фабрики леща и тунца
/// </summary>
public class BreamTunaFactoryMethod : FishFactoryMethod
{
  /// <summary>
  /// Создает фабрику для леща и тунца
  /// </summary>
  /// <returns>Фабрика</returns>
  public override IFishFactory CreateFactory()
  {
    return new BreamTunaFactory();
  }
}