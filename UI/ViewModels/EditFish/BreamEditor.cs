using Duz_vadim_project;

namespace UI.ViewModels.EditFish;


/// <summary>
/// Модель представления для редактирования сведений о лещах.
/// </summary>
public partial class BreamEditor : FishEditor<Bream>
{
    /// <summary>
    /// Создаёт форму редактирования без входных данных.
    /// </summary>
    public BreamEditor() : base(null, false)
    {
    }

    /// <summary>
    /// Создаёт форму редактирования с учётом переданного экземпляра.
    /// </summary>
    /// <param name="instance">Редактируемая запись, допускающая значение null.</param>
    /// <param name="isViewMode">Флаг режима просмотра.</param>
    public BreamEditor(Bream? instance, bool isViewMode) : base(instance, isViewMode)
    {
    }
}