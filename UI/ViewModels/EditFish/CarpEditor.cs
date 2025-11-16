using Duz_vadim_project;

namespace UI.ViewModels.EditFish;

/// <summary>
/// Модель представления для редактирования сведений о карпах.
/// </summary>
public partial class CarpEditor : FishEditor<Carp>
{
    /// <summary>
    /// Создаёт форму редактирования без входных данных.
    /// </summary>
    public CarpEditor() : base(null, false)
    {
    }

    /// <summary>
    /// Создаёт форму редактирования с учётом переданного экземпляра.
    /// </summary>
    /// <param name="instance">Редактируемая запись, допускающая значение null.</param>
    /// <param name="isViewMode">Флаг режима просмотра.</param>
    public CarpEditor(Carp? instance, bool isViewMode) : base(instance, isViewMode)
    {
    }
}