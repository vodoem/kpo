using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Duz_vadim_project;
using Duz_vadim_project.DesignPatterns;
using Duz_vadim_project.DesignPatterns.FactoryMethod;
using Duz_vadim_project.DesignPatterns.PrototypeFactory;
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
    /// Список рыб
    /// </summary>
    public ObservableCollection<Fish> FishList { get; } = [];

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

        GenerateTestFishCommand = ReactiveCommand.Create(GenerateTestFish);
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
            FishList.Add(result);
            SelectedFish = result;
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
            FishList.Add(result);
            SelectedFish = result;
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
            FishList.Contains(SelectedFish);
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
        if (result != null)
        {
            FishList.Remove(SelectedFish);
        }
    }

    /// <summary>
    /// Генерация тестового списка рыб
    /// </summary>
    private void GenerateTestFish()
    {
        foreach (var fish in FishGenerator.GenerateFishList())
        {
            FishList.Add(fish);
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
}