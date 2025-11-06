using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
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
/// ViewModel главного окна для работы с рыбами
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
    /// Возвращает текущий отфильтрованный список рыб для отображения в таблице.
    /// </summary>
    public IReadOnlyList<Fish> FishList => _filteredFish;

    /// <summary>
    /// Выбранная рыба
    /// </summary>
    private Fish? _selectedFish;

    /// <summary>
    /// Выбранная рыба
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
    /// Возможность редактировать или удалить выбранную рыбу
    /// </summary>
    public bool CanEditOrDelete => SelectedFish != null;

    /// <summary>
    /// Текстовый фильтр по типу рыбы.
    /// </summary>
    private string _typeFilter = string.Empty;

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
    private string _weightMinFilter = string.Empty;

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
    private string _weightMaxFilter = string.Empty;

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
    private string _currentFilterSummary = "Без фильтра";

    /// <summary>
    /// Текущий текст, отображающий активный фильтр.
    /// </summary>
    public string CurrentFilterSummary
    {
        get => _currentFilterSummary;
        private set => this.RaiseAndSetIfChanged(ref _currentFilterSummary, value);
    }

    /// <summary>
    /// Взаимодействие с диалогом редактирования рыбы
    /// </summary>
    public Interaction<ViewModelBase, Fish?> ShowEditFishDialog { get; }

    /// <summary>
    /// Команда для добавления пресноводной рыбы
    /// </summary>
    public ReactiveCommand<Unit, Unit> AddFreshwaterFishCommand { get; }

    /// <summary>
    /// Команда для добавления морской рыбы
    /// </summary>
    public ReactiveCommand<Unit, Unit> AddSaltwaterFishCommand { get; }

    /// <summary>
    /// Команда для редактирования выбранной рыбы
    /// </summary>
    public ReactiveCommand<Unit, Unit> EditCommand { get; }

    /// <summary>
    /// Команда для удаления выбранной рыбы
    /// </summary>
    public ReactiveCommand<Unit, Unit> DeleteCommand { get; }

    /// <summary>
    /// Команда для генерации тестового списка рыб
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
    /// Выбранная фабрика для создания объектов рыб
    /// </summary>
    private IFishFactory? _selectedFactory;

    /// <summary>
    /// Свойство для доступа к выбранной фабрике
    /// </summary>
    public IFishFactory? SelectedFactory
    {
        get => _selectedFactory;
        set => this.RaiseAndSetIfChanged(ref _selectedFactory, value);
    }

    /// <summary>
    /// Конструктор по умолчанию
    /// </summary>
    public MainWindowViewModel()
    {
        ShowEditFishDialog = new Interaction<ViewModelBase, Fish?>();

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
    /// Реализация добавления пресноводной рыбы
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
    /// Реализация добавления морской рыбы
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
    /// Редактирование выбранной рыбы
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
    /// Удаление выбранной рыбы
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
    /// Генерация тестового списка рыб
    /// </summary>
    private async Task GenerateTestFishAsync()
    {
        var progressController = new ProgressFormController();
        progressController.Initialize(
            "Генерация тестовых записей",
            "Подготовка к генерации...",
            FishGenerator.RecordCount,
            ProgressCompletionMode.WaitForUserAction);

        using var cts = new CancellationTokenSource();
        progressController.OperationCancelled += (_, _) => cts.Cancel();

        var shouldCloseForm = false;

        try
        {
            var generatedFish = await Task.Run(
                () => FishGenerator.GenerateFishListAsync(progressController, cts.Token),
                cts.Token);

            if (cts.IsCancellationRequested)
            {
                progressController.ReportProgress(0, "Генерация отменена пользователем.");
                shouldCloseForm = true;
                return;
            }

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                _allFish.Clear();
                _allFish.AddRange(generatedFish);
                ApplyFilter();
            });

            progressController.ReportProgress(0, "Генерация завершена. Нажмите «Закрыть».");
        }
        catch (OperationCanceledException)
        {
            shouldCloseForm = true;
            progressController.ReportProgress(0, "Генерация отменена пользователем.");
        }
        catch (Exception ex)
        {
            shouldCloseForm = true;
            progressController.ReportProgress(0, $"Ошибка: {ex.Message}");
        }
        finally
        {
            if (shouldCloseForm)
            {
                progressController.Close();
            }
        }
    }

    /// <summary>
    /// Инициализация фабрик на основе выбранного индекса
    /// </summary>
    /// <param name="parFactoryIndex">Индекс выбранной фабрики</param>
    /// <exception cref="InvalidOperationException">Выбрана неизвестная фабрика</exception>
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
    /// Создание ViewModel для редактирования рыбы
    /// </summary>
    /// <param name="parFish">Объект рыбы</param>
    /// <param name="parIsViewMode">Флаг режима просмотра</param>
    /// <returns>ViewModel для редактирования рыбы</returns>
    /// <exception cref="InvalidOperationException">Неизвестный тип рыбы</exception>
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

    private void ApplyFilter(Fish? preferredSelection = null)
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

        _filteredFish = query.ToList();
        this.RaisePropertyChanged(nameof(FishList));

        UpdateFilterSummary(filter, minWeight, maxWeight);

        var selectionToRestore = preferredSelection ?? SelectedFish;
        if (selectionToRestore != null && _filteredFish.Contains(selectionToRestore))
        {
            var previous = selectionToRestore;
            SelectedFish = null;
            SelectedFish = previous;
        }
        else
        {
            SelectedFish = _filteredFish.FirstOrDefault();
        }
    }

    private void ClearFilter()
    {
        TypeFilter = string.Empty;
        WeightMinFilter = string.Empty;
        WeightMaxFilter = string.Empty;
        ApplyFilter();
    }

    private void UpdateFilterSummary(string filter, decimal? minWeight, decimal? maxWeight)
    {
        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(filter))
        {
            parts.Add($"Тип содержит \"{filter}\"");
        }

        if (minWeight.HasValue && maxWeight.HasValue)
        {
            if (minWeight.Value == maxWeight.Value)
            {
                parts.Add($"Вес = {minWeight.Value.ToString("N2", CultureInfo.CurrentCulture)} г");
            }
            else
            {
                parts.Add($"Вес {minWeight.Value.ToString("N2", CultureInfo.CurrentCulture)}–{maxWeight.Value.ToString("N2", CultureInfo.CurrentCulture)} г");
            }
        }
        else if (minWeight.HasValue)
        {
            parts.Add($"Вес ≥ {minWeight.Value.ToString("N2", CultureInfo.CurrentCulture)} г");
        }
        else if (maxWeight.HasValue)
        {
            parts.Add($"Вес ≤ {maxWeight.Value.ToString("N2", CultureInfo.CurrentCulture)} г");
        }

        CurrentFilterSummary = parts.Count == 0 ? "Без фильтра" : string.Join("; ", parts);
    }

    private static decimal? TryParseDecimal(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (decimal.TryParse(value, NumberStyles.Number, CultureInfo.CurrentCulture, out var result))
        {
            return result;
        }

        if (decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out result))
        {
            return result;
        }

        return null;
    }
}
