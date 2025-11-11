using Duz_vadim_project;

namespace UI.ViewModels.EditFish;


/// <summary>
/// Модель представления для редактирования сведений о тунцах.
/// </summary>
public partial class TunaEditor : FishEditor<Tuna>
{
    /// <summary>
    /// Создаёт форму редактирования без входных данных.
    /// </summary>
    public TunaEditor() : base(null, false)
    {
    }

    /// <summary>
    /// Создаёт форму редактирования с учётом переданного экземпляра.
    /// </summary>
    /// <param name="instance">Редактируемая запись, допускающая значение null.</param>
    /// <param name="isViewMode">Флаг режима просмотра.</param>
    public TunaEditor(Tuna? instance, bool isViewMode) : base(instance, isViewMode)
    {
    }
}