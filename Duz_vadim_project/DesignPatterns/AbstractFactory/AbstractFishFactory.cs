namespace Duz_vadim_project.DesignPatterns.AbstractFishFactory;

/// <summary>
/// Абстрактная фабрика для создания объектов рыб
/// </summary>
public abstract class AbstractFishFactory : IFishFactory
{
    /// <summary>
    /// Создает объект пресноводной рыбы
    /// </summary>
    /// <returns>Экземпляр пресноводной рыбы</returns>
    public abstract FreshwaterFish CreateFreshwaterFish();

    /// <summary>
    /// Создает объект морской рыбы
    /// </summary>
    /// <returns>Экземпляр морской рыбы</returns>
    public abstract SaltwaterFish CreateSaltwaterFish();
}