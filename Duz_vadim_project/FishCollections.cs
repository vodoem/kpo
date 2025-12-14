namespace Duz_vadim_project;

/// <summary>
/// Набор коллекций рыб, соответствующий схеме OpenAPI.
/// </summary>
public class FishCollections
{
  /// <summary>
  /// Все карпы.
  /// </summary>
  public List<Carp> Carps { get; set; } = new();

  /// <summary>
  /// Все скумбрии.
  /// </summary>
  public List<Mackerel> Mackerels { get; set; } = new();
}
