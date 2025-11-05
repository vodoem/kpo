using System.Collections.Generic;
using Duz_vadim_project;

namespace UI.Generator;

/// <summary>
/// Генератор рыб
/// </summary>
public class FishGenerator
{
  /// <summary>
  /// Генерирует список рыб
  /// </summary>
  /// <returns>Список рыб</returns>
  public static List<Fish> GenerateFishList()
  {
    var fishList = new List<Fish>
    {
      // Лещи
      new Bream(1200.5m, 3, true, 5.5m, "Овальная"),
      new Bream(1500.0m, 4, true, 6.0m, "Вытянутая"),

      // Карпы
      new Carp(2000.0m, 5, true, 4.0m, "Золотистый"),
      new Carp(1800.0m, 6, true, 4.5m, "Серебристый"),

      // Скумбрии
      new Mackerel(800.0m, 2, true, 35.0m, 2.5m),
      new Mackerel(900.0m, 3, true, 36.0m, 3.0m),

      // Тунцы
      new Tuna(5000.0m, 7, true, 34.5m, 70.0m),
      new Tuna(5500.0m, 8, true, 35.0m, 75.0m)
    };

    return fishList;
  }
}
