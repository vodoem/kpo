using Duz_vadim_project;

namespace UI.ViewModels.EditFish;

/// <summary>
/// Модель представления для редактирования сведений о скумбрии.
/// </summary>
public partial class MackerelEditor : FishEditor<Mackerel>
{
    /// <summary>
    /// Создаёт форму редактирования без входных данных.
    /// </summary>
    public MackerelEditor() : base(null, false)
    {
    }

    /// <summary>
    /// Создаёт форму редактирования с учётом переданного экземпляра.
    /// </summary>
    /// <param name="instance">Редактируемая запись, допускающая значение null.</param>
    /// <param name="isViewMode">Флаг режима просмотра.</param>
    public MackerelEditor(Mackerel? instance, bool isViewMode) : base(instance, isViewMode)
    {
    }
}