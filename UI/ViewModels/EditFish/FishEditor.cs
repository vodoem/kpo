using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Duz_vadim_project;
using ReactiveUI;

namespace UI.ViewModels.EditFish;

/// <summary>
/// Модель представления для редактирования любого вида рыбы.
/// </summary>
/// <typeparam name="TFish">Тип редактируемой рыбы.</typeparam>
public partial class FishEditor<TFish> : ViewModelBase where TFish : Fish, ICloneable
{
    private readonly TFish _fishInstance;
    private readonly bool _isViewMode;
    private string? _errorMessage;

    /// <summary>
    /// Делегат для сохранения изменений.
    /// </summary>
    public Func<TFish, Task<SaveOperationResult>>? SaveHandler { get; set; }

    /// <summary>
    /// Текущий экземпляр рыбы.
    /// </summary>
    public TFish FishInstance => _fishInstance;

    /// <summary>
    /// Флаг, определяющий, доступна ли форма только для просмотра.
    /// </summary>
    public bool IsViewMode => _isViewMode;

    /// <summary>
    /// Заголовок окна.
    /// </summary>
    public string WindowTitle => _isViewMode ? "Просмотр данных о рыбе" : $"Редактирование {FishInstance.GetType().Name}";

    /// <summary>
    /// Команда сохранения изменений.
    /// </summary>
    public ReactiveCommand<Unit, TFish?> SaveChanges { get; }

    /// <summary>
    /// Текст ошибки, который следует показать пользователю.
    /// </summary>
    public string? ErrorMessage
    {
        get => _errorMessage;
        private set => this.RaiseAndSetIfChanged(ref _errorMessage, value);
    }

    /// <summary>
    /// Создаёт форму без предварительных данных.
    /// </summary>
    public FishEditor() : this(null, false)
    {
    }

    /// <summary>
    /// Создаёт форму с переданным экземпляром рыбы.
    /// </summary>
    /// <param name="instance">Экземпляр рыбы, который может быть равен null.</param>
    /// <param name="viewMode">Флаг режима просмотра.</param>
    public FishEditor(TFish? instance, bool viewMode)
    {
        _fishInstance = instance?.Clone() as TFish ?? Activator.CreateInstance<TFish>();
        _isViewMode = viewMode;

        SaveChanges = ReactiveCommand.CreateFromTask(SaveAsync);
    }

    private async Task<TFish?> SaveAsync()
    {
        ErrorMessage = null;

        if (_isViewMode || SaveHandler is null)
        {
            return _fishInstance;
        }

        var result = await SaveHandler(_fishInstance);
        if (result.Success)
        {
            return _fishInstance;
        }

        ErrorMessage = result.ErrorMessage ?? "Не удалось сохранить изменения.";
        return null;
    }
}

/// <summary>
/// Результат сохранения из формы редактирования.
/// </summary>
/// <param name="Success">Признак успеха.</param>
/// <param name="ErrorMessage">Описание ошибки.</param>
public record SaveOperationResult(bool Success, string? ErrorMessage = null);