using Duz_vadim_project;

namespace UI.ViewModels.EditFish;

/// <summary>
/// ViewModel для редактирования скумбрии.
/// </summary>
public partial class MackerelEditor : FishEditor<Mackerel>
{
  /// <summary>
  /// Конструктор по умолчанию
  /// </summary>
  public MackerelEditor() : base(null, false) { }

  /// <summary>
  /// Конструктор с параметрами
  /// </summary>
  /// <param name="instance">Редактируемый экземпляр</param>
  /// <param name="isViewMode">Флаг режима просмотра</param>
  public MackerelEditor(Mackerel? instance, bool isViewMode) : base(instance, isViewMode) { }
}