using Duz_vadim_project;

namespace UI.ViewModels.EditFish;

/// <summary>
/// ViewModel для редактирования карпа.
/// </summary>
public partial class CarpEditor : FishEditor<Carp>
{
  /// <summary>
  /// Конструктор по умолчанию
  /// </summary>
  public CarpEditor() : base(null, false) { }

  /// <summary>
  /// Конструктор с параметрами
  /// </summary>
  /// <param name="instance">Редактируемый экземпляр</param>
  /// <param name="isViewMode">Флаг режима просмотра</param>
  public CarpEditor(Carp? instance, bool isViewMode) : base(instance, isViewMode) { }
}