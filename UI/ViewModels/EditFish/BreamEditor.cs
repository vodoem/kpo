using Duz_vadim_project;

namespace UI.ViewModels.EditFish;


/// <summary>
/// ViewModel для редактирования леща.
/// </summary>
public partial class BreamEditor : FishEditor<Bream>
{
  /// <summary>
  /// Конструктор по умолчанию
  /// </summary>
  public BreamEditor() : base(null, false) { }

  /// <summary>
  /// Конструктор с параметрами
  /// </summary>
  /// <param name="instance">Редактируемый экземпляр</param>
  /// <param name="isViewMode">Флаг режима просмотра</param>
  public BreamEditor(Bream? instance, bool isViewMode) : base(instance, isViewMode) { }
}