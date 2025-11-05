namespace Duz_vadim_project.DesignPatterns;

/// <summary>
/// Интерфейс фабрики для создания объектов рыб
/// </summary>
public interface IFishFactory
{
    /// <summary>
    /// Создает объект пресноводной рыбы
    /// </summary>
    /// <returns>Экземпляр пресноводной рыбы</returns>
    FreshwaterFish CreateFreshwaterFish();

    /// <summary>
    /// Создает объект морской рыбы
    /// </summary>
    /// <returns>Экземпляр морской рыбы</returns>
    SaltwaterFish CreateSaltwaterFish();
}