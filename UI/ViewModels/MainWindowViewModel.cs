using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using Duz_vadim_project;
using Duz_vadim_project.DesignPatterns;
using Duz_vadim_project.DesignPatterns.FactoryMethod;
using Duz_vadim_project.DesignPatterns.PrototypeFactory;
using ProgressDisplay;
using ReactiveUI;
using UI.Generator;
using UI.ViewModels.EditFish;

namespace UI.ViewModels;

/// <summary>
/// Модель представления главного окна для работы с рыбами.
/// </summary>
public partial class MainWindowViewModel : ViewModelBase
{
  /// <summary>
  /// Полный набор записей о рыбах.
  /// </summary>
  private readonly List<Fish> _allFish = new();

  /// <summary>
  /// Текущий отфильтрованный список рыб.
  /// </summary>
  private List<Fish> _filteredFish = new();

  /// <summary>
  /// Выбранная рыба.
  /// </summary>
  private Fish? _selectedFish;

  /// <summary>
  /// Текстовый фильтр по типу рыбы.
  /// </summary>
  private string _typeFilter = string.Empty;

  /// <summary>
  /// Минимальный вес для фильтрации.
  /// </summary>
  private string _weightMinFilter = string.Empty;

  /// <summary>
  /// Максимальный вес для фильтрации.
  /// </summary>
  private string _weightMaxFilter = string.Empty;

  /// <summary>
  /// Текущий текст, отображающий активный фильтр.
  /// </summary>
  private string _currentFilterSummary = "Без фильтра";

  /// <summary>
  /// Признак того, что панель фильтров видима пользователю.
  /// </summary>
  private bool _isFilterVisible = true;

  /// <summary>
  /// Выбранная фабрика для создания объектов рыб.
  /// </summary>
  private IFishFactory? _selectedFactory;

  /// <summary>
  /// Возвращает текущий отфильтрованный список рыб для отображения в таблице.
  /// </summary>
  public IReadOnlyList<Fish> FishList => _filteredFish;

  /// <summary>
  /// Выбранная рыба.
  /// </summary>
  public Fish? SelectedFish
  {
    get => _selectedFish;
    set
    {
      this.RaiseAndSetIfChanged(ref _selectedFish, value);
      this.RaisePropertyChanged(nameof(CanEditOrDelete));
    }
  }

  /// <summary>
  /// Возможность редактировать или удалить выбранную рыбу.
  /// </summary>
  public bool CanEditOrDelete => SelectedFish != null;

  /// <summary>
  /// Текстовый фильтр по типу рыбы.
  /// </summary>
  public string TypeFilter
  {
    get => _typeFilter;
    set => this.RaiseAndSetIfChanged(ref _typeFilter, value);
  }

  /// <summary>
  /// Минимальный вес для фильтрации.
  /// </summary>
  public string WeightMinFilter
  {
    get => _weightMinFilter;
    set => this.RaiseAndSetIfChanged(ref _weightMinFilter, value);
  }

  /// <summary>
  /// Максимальный вес для фильтрации.
  /// </summary>
  public string WeightMaxFilter
  {
    get => _weightMaxFilter;
    set => this.RaiseAndSetIfChanged(ref _weightMaxFilter, value);
  }

  /// <summary>
  /// Текущий текст, отображающий активный фильтр.
  /// </summary>
  public string CurrentFilterSummary
  {
    get => _currentFilterSummary;
    private set
    {
      this.RaiseAndSetIfChanged(ref _currentFilterSummary, value);
      this.RaisePropertyChanged(nameof(CollapsedFilterSummary));
    }
  }

  /// <summary>
  /// Компактное представление фильтра для свёрнутого состояния панели.
  /// </summary>
  public string CollapsedFilterSummary => $"Фильтр: {CurrentFilterSummary}";

  /// <summary>
  /// Признак того, что панель фильтрации отображается пользователю.
  /// </summary>
  public bool IsFilterVisible
  {
    get => _isFilterVisible;
    set
    {
      if (this.RaiseAndSetIfChanged(ref _isFilterVisible, value))
      {
        this.RaisePropertyChanged(nameof(IsFilterCollapsed));
        this.RaisePropertyChanged(nameof(FilterToggleText));
        this.RaisePropertyChanged(nameof(ShowCollapsedFilterSummary));
      }
    }
  }

  /// <summary>
  /// Признак того, что панель фильтрации скрыта.
  /// </summary>
  public bool IsFilterCollapsed => !IsFilterVisible;

  /// <summary>
  /// Признак того, что компактное описание фильтра должно отображаться.
  /// </summary>
  public bool ShowCollapsedFilterSummary => !IsFilterVisible;

  /// <summary>
  /// Текст заголовка сворачиваемой панели фильтрации.
  /// </summary>
  public string FilterToggleText => IsFilterVisible ? "Скрыть фильтр" : "Показать фильтр";

  /// <summary>
  /// Взаимодействие с диалоговым окном редактирования рыбы.
  /// </summary>
  public Interaction<ViewModelBase, Fish?> ShowEditFishDialog { get; }

  /// <summary>
  /// Запрос фокусировки на выбранной записи в таблице.
  /// </summary>
  public Interaction<Fish, Unit> FocusFishRequest { get; }

  /// <summary>
  /// Команда для добавления пресноводной рыбы.
  /// </summary>
  public ReactiveCommand<Unit, Unit> AddFreshwaterFishCommand { get; }

  /// <summary>
  /// Команда для добавления морской рыбы.
  /// </summary>
  public ReactiveCommand<Unit, Unit> AddSaltwaterFishCommand { get; }

  /// <summary>
  /// Команда для редактирования выбранной рыбы.
  /// </summary>
  public ReactiveCommand<Unit, Unit> EditCommand { get; }

  /// <summary>
  /// Команда для удаления выбранной рыбы.
  /// </summary>
  public ReactiveCommand<Unit, Unit> DeleteCommand { get; }

  /// <summary>
  /// Команда для генерации тестового списка рыб.
  /// </summary>
  public ReactiveCommand<Unit, Unit> GenerateTestFishCommand { get; }

  /// <summary>
  /// Команда применения фильтра.
  /// </summary>
  public ReactiveCommand<Unit, Unit> ApplyFilterCommand { get; }

  /// <summary>
  /// Команда сброса фильтра.
  /// </summary>
  public ReactiveCommand<Unit, Unit> ClearFilterCommand { get; }

  /// <summary>
  /// Свойство для доступа к выбранной фабрике.
  /// </summary>
  public IFishFactory? SelectedFactory
  {
    get => _selectedFactory;
    set => this.RaiseAndSetIfChanged(ref _selectedFactory, value);
  }

  /// <summary>
  /// Конструктор по умолчанию.
  /// </summary>
  public MainWindowViewModel()
  {
    ShowEditFishDialog = new Interaction<ViewModelBase, Fish?>();
    FocusFishRequest = new Interaction<Fish, Unit>();

    AddFreshwaterFishCommand = ReactiveCommand.CreateFromTask(AddFreshwaterFishImplementation);
    AddSaltwaterFishCommand = ReactiveCommand.CreateFromTask(AddSaltwaterFishImplementation);

    EditCommand = ReactiveCommand.CreateFromTask(EditFishCommand, this.WhenAnyValue(parX => parX.CanEditOrDelete));
    DeleteCommand = ReactiveCommand.CreateFromTask(DeleteFishAsync, this.WhenAnyValue(parX => parX.CanEditOrDelete));

    GenerateTestFishCommand = ReactiveCommand.CreateFromTask(GenerateTestFishAsync);
    ApplyFilterCommand = ReactiveCommand.Create(() => ApplyFilter());
    ClearFilterCommand = ReactiveCommand.Create(ClearFilter);
    InitializeFactories(0);
    ApplyFilter();
  }

  /// <summary>
  /// Реализация добавления пресноводной рыбы.
  /// </summary>
  private async Task AddFreshwaterFishImplementation()
  {
    SelectedFactory ??= new CarpMackerelFactoryMethod().CreateFactory();

    var fish = SelectedFactory.CreateFreshwaterFish();
    var viewModel = CreateEditor(fish, false);
    var result = await ShowEditFishDialog.Handle(viewModel);

    if (result != null)
    {
      _allFish.Add(result);
      ApplyFilter(result);
    }
  }

  /// <summary>
  /// Реализация добавления морской рыбы.
  /// </summary>
  private async Task AddSaltwaterFishImplementation()
  {
    SelectedFactory ??= new CarpMackerelFactoryMethod().CreateFactory();

    var fish = SelectedFactory.CreateSaltwaterFish();
    var viewModel = CreateEditor(fish, false);
    var result = await ShowEditFishDialog.Handle(viewModel);

    if (result != null)
    {
      _allFish.Add(result);
      ApplyFilter(result);
    }
  }

  /// <summary>
  /// Редактирование выбранной рыбы.
  /// </summary>
  private async Task EditFishCommand()
  {
    if (SelectedFish == null)
    {
      return;
    }

    ViewModelBase? editViewModel = SelectedFish switch
    {
      Bream bream => new FishEditor<Bream>(bream, false),
      Carp carp => new FishEditor<Carp>(carp, false),
      Mackerel mackerel => new FishEditor<Mackerel>(mackerel, false),
      Tuna tuna => new FishEditor<Tuna>(tuna, false),
      _ => null
    };

    if (editViewModel is null)
    {
      return;
    }

    var result = await ShowEditFishDialog.Handle(editViewModel);
    if (result != null)
    {
      SelectedFish.CopyFrom(result);
    }
  }

  /// <summary>
  /// Удаление выбранной рыбы.
  /// </summary>
  private async Task DeleteFishAsync()
  {
    if (SelectedFish == null)
    {
      return;
    }

    ViewModelBase? deleteViewModel = SelectedFish switch
    {
      Bream bream => new FishEditor<Bream>(bream, true),
      Carp carp => new FishEditor<Carp>(carp, true),
      Mackerel mackerel => new FishEditor<Mackerel>(mackerel, true),
      Tuna tuna => new FishEditor<Tuna>(tuna, true),
      _ => null
    };

    if (deleteViewModel is null)
    {
      return;
    }

    var result = await ShowEditFishDialog.Handle(deleteViewModel);
    if (result != null && SelectedFish != null)
    {
      _allFish.Remove(SelectedFish);
      ApplyFilter();
    }
  }

  /// <summary>
  /// Генерация тестового списка рыб.
  /// </summary>
  private async Task GenerateTestFishAsync()
  {
    var options = new ProgressExecutionOptions(
      "Генерация тестовых записей",
      "Подготовка к генерации...",
      FishGenerator.RecordCount,
      ProgressCompletionMode.AutoClose);

    var progressController = new ProgressFormController();

    List<Fish>? generatedFish = null;

    try
    {
      generatedFish = await progressController.RunAsync(
        options,
        async session => await FishGenerator.GenerateFishListAsync(session).ConfigureAwait(false)).ConfigureAwait(false);
    }
    catch (OperationCanceledException)
    {
      return;
    }
    catch (Exception)
    {
      return;
    }

    if (generatedFish == null)
    {
      return;
    }

    await Dispatcher.UIThread.InvokeAsync(
      () =>
      {
        _allFish.Clear();
        _allFish.AddRange(generatedFish);
        ApplyFilter(_allFish.FirstOrDefault());
      });
  }

  /// <summary>
  /// Инициализация фабрик на основе выбранного индекса.
  /// </summary>
  /// <param name="parFactoryIndex">Индекс выбранной фабрики.</param>
  /// <exception cref="InvalidOperationException">Выбрана неизвестная фабрика.</exception>
  public void InitializeFactories(int parFactoryIndex)
  {
    SelectedFactory = parFactoryIndex switch
    {
      0 => new CarpMackerelFactoryMethod().CreateFactory(),
      1 => new BreamTunaFactoryMethod().CreateFactory(),
      2 => new BreamTunaPrototypeFactoryMethod().CreateFactory(),
      3 => new CarpMackerelPrototypeFactoryMethod().CreateFactory(),
      _ => throw new InvalidOperationException("Неизвестная фабрика")
    };
  }

  /// <summary>
  /// Создание модели представления для редактирования рыбы.
  /// </summary>
  /// <param name="parFish">Объект рыбы.</param>
  /// <param name="parIsViewMode">Флаг режима просмотра.</param>
  /// <returns>Модель представления для редактирования рыбы.</returns>
  /// <exception cref="InvalidOperationException">Неизвестный тип рыбы.</exception>
  public static ViewModelBase CreateEditor(Fish parFish, bool parIsViewMode)
  {
    return parFish switch
    {
      Bream bream => new FishEditor<Bream>(bream, parIsViewMode),
      Carp carp => new FishEditor<Carp>(carp, parIsViewMode),
      Mackerel mackerel => new FishEditor<Mackerel>(mackerel, parIsViewMode),
      Tuna tuna => new FishEditor<Tuna>(tuna, parIsViewMode),
      _ => throw new InvalidOperationException("Неизвестный тип рыбы")
    };
  }

  /// <summary>
  /// Применяет текущие значения фильтра и обновляет отображаемый список.
  /// </summary>
  /// <param name="parPreferredSelection">Объект рыбы, которому следует вернуть фокус после фильтрации.</param>
  private void ApplyFilter(Fish? parPreferredSelection = null)
  {
    var filter = TypeFilter?.Trim() ?? string.Empty;
    var minWeight = TryParseDecimal(WeightMinFilter);
    var maxWeight = TryParseDecimal(WeightMaxFilter);

    if (minWeight.HasValue && maxWeight.HasValue && minWeight > maxWeight)
    {
      (minWeight, maxWeight) = (maxWeight, minWeight);
    }

    IEnumerable<Fish> query = _allFish;

    if (!string.IsNullOrWhiteSpace(filter))
    {
      query = query.Where(fish => fish.TypeName.Contains(filter, StringComparison.OrdinalIgnoreCase));
    }

    if (minWeight.HasValue || maxWeight.HasValue)
    {
      query = query.Where(fish =>
      {
        var weight = fish.Weight;
        if (minWeight.HasValue && weight < minWeight.Value)
        {
          return false;
        }

        if (maxWeight.HasValue && weight > maxWeight.Value)
        {
          return false;
        }

        return true;
      });
    }

    var selectionToRestore = parPreferredSelection ?? SelectedFish;

    _filteredFish = query.ToList();
    this.RaisePropertyChanged(nameof(FishList));

    UpdateFilterSummary(filter, minWeight, maxWeight);

    if (selectionToRestore != null)
    {
      var restoredSelection = _filteredFish.FirstOrDefault(parFish => ReferenceEquals(parFish, selectionToRestore));
      if (restoredSelection != null)
      {
        SelectedFish = null;
        SelectedFish = restoredSelection;
        _ = FocusFishRequest.Handle(restoredSelection);
        return;
      }
    }

    var defaultSelection = _filteredFish.FirstOrDefault();
    SelectedFish = defaultSelection;

    if (defaultSelection != null)
    {
      _ = FocusFishRequest.Handle(defaultSelection);
    }
  }

  /// <summary>
  /// Сбрасывает значения фильтров к настройкам по умолчанию.
  /// </summary>
  private void ClearFilter()
  {
    TypeFilter = string.Empty;
    WeightMinFilter = string.Empty;
    WeightMaxFilter = string.Empty;
    ApplyFilter();
  }

  /// <summary>
  /// Обновляет текстовое описание активного фильтра.
  /// </summary>
  /// <param name="parFilter">Текстовый фильтр по типу рыбы.</param>
  /// <param name="parMinWeight">Минимальный вес.</param>
  /// <param name="parMaxWeight">Максимальный вес.</param>
  private void UpdateFilterSummary(string parFilter, decimal? parMinWeight, decimal? parMaxWeight)
  {
    var parts = new List<string>();

    if (!string.IsNullOrWhiteSpace(parFilter))
    {
      parts.Add($"Тип содержит \"{parFilter}\"");
    }

    if (parMinWeight.HasValue && parMaxWeight.HasValue)
    {
      if (parMinWeight.Value == parMaxWeight.Value)
      {
        parts.Add($"Вес = {parMinWeight.Value.ToString("N2", CultureInfo.CurrentCulture)} г");
      }
      else
      {
        parts.Add($"Вес {parMinWeight.Value.ToString("N2", CultureInfo.CurrentCulture)}–{parMaxWeight.Value.ToString("N2", CultureInfo.CurrentCulture)} г");
      }
    }
    else if (parMinWeight.HasValue)
    {
      parts.Add($"Вес ≥ {parMinWeight.Value.ToString("N2", CultureInfo.CurrentCulture)} г");
    }
    else if (parMaxWeight.HasValue)
    {
      parts.Add($"Вес ≤ {parMaxWeight.Value.ToString("N2", CultureInfo.CurrentCulture)} г");
    }

    CurrentFilterSummary = parts.Count == 0 ? "Без фильтра" : string.Join("; ", parts);
  }

  /// <summary>
  /// Пытается преобразовать строку в десятичное число с учётом культуры.
  /// </summary>
  /// <param name="parValue">Исходная строка.</param>
  /// <returns>Числовое значение или <c>null</c>, если преобразование не удалось.</returns>
  private static decimal? TryParseDecimal(string parValue)
  {
    if (string.IsNullOrWhiteSpace(parValue))
    {
      return null;
    }

    if (decimal.TryParse(parValue, NumberStyles.Number, CultureInfo.CurrentCulture, out var result))
    {
      return result;
    }

    if (decimal.TryParse(parValue, NumberStyles.Number, CultureInfo.InvariantCulture, out result))
    {
      return result;
    }

    return null;
  }
}
