using Duz_vadim_project;

namespace Server;

/// <summary>
/// Потокобезопасное хранилище рыб с сохранением в JSON.
/// </summary>
public class FishRepository
{
  private readonly string _storagePath;
  private readonly SemaphoreSlim _sync = new(1, 1);
  private FishCollections _state = new();
  private int _nextId = 1;

  /// <summary>
  /// Создаёт новый экземпляр хранилища.
  /// </summary>
  /// <param name="environment">Хостовая среда для определения путей.</param>
  public FishRepository(IHostEnvironment environment)
  {
    _storagePath = Path.Combine(environment.ContentRootPath, "fish-data.json");
    LoadState();
  }

  /// <summary>
  /// Возвращает копию всех сохранённых рыб.
  /// </summary>
  public async Task<FishCollections> GetCollectionsAsync()
  {
    await _sync.WaitAsync();
    try
    {
      return CloneState(_state);
    }
    finally
    {
      _sync.Release();
    }
  }

  /// <summary>
  /// Получает карпа по идентификатору.
  /// </summary>
  public async Task<Carp?> GetCarpAsync(int id)
  {
    await _sync.WaitAsync();
    try
    {
      return _state.Carps.FirstOrDefault(fish => fish.Id == id)?.Clone() as Carp;
    }
    finally
    {
      _sync.Release();
    }
  }

  /// <summary>
  /// Получает скумбрию по идентификатору.
  /// </summary>
  public async Task<Mackerel?> GetMackerelAsync(int id)
  {
    await _sync.WaitAsync();
    try
    {
      return _state.Mackerels.FirstOrDefault(fish => fish.Id == id)?.Clone() as Mackerel;
    }
    finally
    {
      _sync.Release();
    }
  }

  /// <summary>
  /// Добавляет нового карпа, присваивая идентификатор сервера.
  /// </summary>
  public async Task<Carp> AddCarpAsync(Carp carp)
  {
    var stored = PrepareForStorage(carp);

    await _sync.WaitAsync();
    try
    {
      stored.Id = _nextId++;
      _state.Carps.Add(stored);
      await SaveAsync();
      return CloneFish(stored);
    }
    finally
    {
      _sync.Release();
    }
  }

  /// <summary>
  /// Добавляет новую скумбрию, присваивая идентификатор сервера.
  /// </summary>
  public async Task<Mackerel> AddMackerelAsync(Mackerel mackerel)
  {
    var stored = PrepareForStorage(mackerel);

    await _sync.WaitAsync();
    try
    {
      stored.Id = _nextId++;
      _state.Mackerels.Add(stored);
      await SaveAsync();
      return CloneFish(stored);
    }
    finally
    {
      _sync.Release();
    }
  }

  /// <summary>
  /// Обновляет данные карпа.
  /// </summary>
  public async Task<Carp?> UpdateCarpAsync(int id, Carp carp)
  {
    var stored = PrepareForStorage(carp);
    stored.Id = id;

    await _sync.WaitAsync();
    try
    {
      var existing = _state.Carps.FirstOrDefault(fish => fish.Id == id);
      if (existing is null)
      {
        return null;
      }

      existing.CopyFrom(stored);
      await SaveAsync();
      return CloneFish(existing);
    }
    finally
    {
      _sync.Release();
    }
  }

  /// <summary>
  /// Обновляет данные скумбрии.
  /// </summary>
  public async Task<Mackerel?> UpdateMackerelAsync(int id, Mackerel mackerel)
  {
    var stored = PrepareForStorage(mackerel);
    stored.Id = id;

    await _sync.WaitAsync();
    try
    {
      var existing = _state.Mackerels.FirstOrDefault(fish => fish.Id == id);
      if (existing is null)
      {
        return null;
      }

      existing.CopyFrom(stored);
      await SaveAsync();
      return CloneFish(existing);
    }
    finally
    {
      _sync.Release();
    }
  }

  /// <summary>
  /// Удаляет карпа по идентификатору.
  /// </summary>
  public async Task<bool> DeleteCarpAsync(int id)
  {
    await _sync.WaitAsync();
    try
    {
      var removed = _state.Carps.RemoveAll(fish => fish.Id == id) > 0;
      if (removed)
      {
        await SaveAsync();
      }

      return removed;
    }
    finally
    {
      _sync.Release();
    }
  }

  /// <summary>
  /// Удаляет скумбрию по идентификатору.
  /// </summary>
  public async Task<bool> DeleteMackerelAsync(int id)
  {
    await _sync.WaitAsync();
    try
    {
      var removed = _state.Mackerels.RemoveAll(fish => fish.Id == id) > 0;
      if (removed)
      {
        await SaveAsync();
      }

      return removed;
    }
    finally
    {
      _sync.Release();
    }
  }

  private void LoadState()
  {
    if (!File.Exists(_storagePath))
    {
      _state = new FishCollections();
      return;
    }

    var json = File.ReadAllText(_storagePath);
    var loaded = System.Text.Json.JsonSerializer.Deserialize<FishCollections>(json);
    _state = loaded ?? new FishCollections();
    _nextId = _state.Carps.Cast<Fish>().Concat(_state.Mackerels).Select(fish => fish.Id).DefaultIfEmpty(0).Max() + 1;
  }

  private async Task SaveAsync()
  {
    var directory = Path.GetDirectoryName(_storagePath);
    if (!string.IsNullOrWhiteSpace(directory))
    {
      Directory.CreateDirectory(directory);
    }

    var json = System.Text.Json.JsonSerializer.Serialize(_state, new System.Text.Json.JsonSerializerOptions
    {
      WriteIndented = true
    });
    await File.WriteAllTextAsync(_storagePath, json);
  }

  private static FishCollections CloneState(FishCollections source) => new()
  {
    Carps = source.Carps.Select(CloneFish).ToList(),
    Mackerels = source.Mackerels.Select(CloneFish).ToList()
  };

  private static T PrepareForStorage<T>(T fish) where T : Fish
  {
    return CloneFish(fish);
  }

  private static T CloneFish<T>(T fish) where T : Fish
  {
    return (T)fish.Clone();
  }
}
