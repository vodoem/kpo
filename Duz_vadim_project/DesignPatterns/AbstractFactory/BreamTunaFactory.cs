namespace Duz_vadim_project.DesignPatterns.AbstractFishFactory;

/// <summary>
/// Конкретная фабрика для создания леща и тунца
/// </summary>
public class BreamTunaFactory : AbstractFishFactory
{
  /// <summary>
  /// Создает объект пресноводной рыбы (лещ)
  /// </summary>
  /// <returns>Экземпляр леща</returns>
  public override FreshwaterFish CreateFreshwaterFish()
  {
    return new Bream();
  }

  /// <summary>
  /// Создает объект морской рыбы (тунец)
  /// </summary>
  /// <returns>Экземпляр тунца</returns>
  public override SaltwaterFish CreateSaltwaterFish()
  {
    return new Tuna();
  }
}