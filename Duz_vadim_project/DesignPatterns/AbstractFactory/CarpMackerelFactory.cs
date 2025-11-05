namespace Duz_vadim_project.DesignPatterns.AbstractFishFactory;

/// <summary>
/// Конкретная фабрика для создания карпа и скумбрии
/// </summary>
public class CarpMackerelFactory : AbstractFishFactory
{
    /// <summary>
    /// Создает объект пресноводной рыбы (карп)
    /// </summary>
    /// <returns>Экземпляр карпа</returns>
    public override FreshwaterFish CreateFreshwaterFish()
    {
        return new Carp();
    }

    /// <summary>
    /// Создает объект морской рыбы (скумбрия)
    /// </summary>
    /// <returns>Экземпляр скумбрии</returns>
    public override SaltwaterFish CreateSaltwaterFish()
    {
        return new Mackerel();
    }
}