using Duz_vadim_project;

namespace UI.ViewModels.EditFish;


/// <summary>
/// ViewModel для редактирования тунца.
/// </summary>
public partial class TunaEditor : FishEditor<Tuna>
{
  /// <summary>
  /// Конструктор по умолчанию
  /// </summary>
  public TunaEditor() : base(null, false) { }

  /// <summary>
  /// Конструктор с параметрами
  /// </summary>
  /// <param name="instance">Редактируемый экземпляр</param>
  /// <param name="isViewMode">Флаг режима просмотра</param>
  public TunaEditor(Tuna? instance, bool isViewMode) : base(instance, isViewMode) { }
}