using System.Text.Json;
using Duz_vadim_project;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
  options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
  options.SerializerOptions.WriteIndented = true;
});

var storagePath = Path.Combine(AppContext.BaseDirectory, "fish-data.json");
var repository = new FishRepository(storagePath);
var app = builder.Build();

app.MapGet("/list", () => Results.Json(repository.GetAll(), repository.JsonOptions));

app.MapGroup("/Carp")
  .MapCarpEndpoints(repository);

app.MapGroup("/Mackerel")
  .MapMackerelEndpoints(repository);

app.Run();

internal static class CarpEndpoints
{
  public static RouteGroupBuilder MapCarpEndpoints(this RouteGroupBuilder group, FishRepository repository)
  {
    group.MapPost("/", (Carp carp) =>
    {
      repository.Add(carp);
      return Results.Created($"/Carp/{carp.Id}", carp);
    });

    group.MapGet("/{id:int}", (int id) =>
    {
      var fish = repository.Find<Carp>(id);
      return fish is null ? Results.NotFound() : Results.Ok(fish);
    });

    group.MapPut("/{id:int}", (int id, Carp carp) =>
    {
      var updated = repository.Update(id, carp);
      return updated is null ? Results.NotFound() : Results.Ok(updated);
    });

    group.MapDelete("/{id:int}", (int id) =>
    {
      return repository.Delete<Carp>(id) ? Results.NoContent() : Results.NotFound();
    });

    return group;
  }
}

internal static class MackerelEndpoints
{
  public static RouteGroupBuilder MapMackerelEndpoints(this RouteGroupBuilder group, FishRepository repository)
  {
    group.MapPost("/", (Mackerel mackerel) =>
    {
      repository.Add(mackerel);
      return Results.Created($"/Mackerel/{mackerel.Id}", mackerel);
    });

    group.MapGet("/{id:int}", (int id) =>
    {
      var fish = repository.Find<Mackerel>(id);
      return fish is null ? Results.NotFound() : Results.Ok(fish);
    });

    group.MapPut("/{id:int}", (int id, Mackerel mackerel) =>
    {
      var updated = repository.Update(id, mackerel);
      return updated is null ? Results.NotFound() : Results.Ok(updated);
    });

    group.MapDelete("/{id:int}", (int id) =>
    {
      return repository.Delete<Mackerel>(id) ? Results.NoContent() : Results.NotFound();
    });

    return group;
  }
}

internal sealed class FishRepository
{
  private readonly string _storagePath;
  private readonly List<Fish> _fish;
  private int _nextId;

  public FishRepository(string storagePath)
  {
    _storagePath = storagePath;
    _fish = Load(storagePath).ToList();
    _nextId = _fish.Count == 0 ? 1 : _fish.Max(f => f.Id) + 1;
  }

  public JsonSerializerOptions JsonOptions { get; } = new()
  {
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    WriteIndented = true
  };

  public IEnumerable<Fish> GetAll() => _fish.Select(CloneForResponse);

  public void Add(Fish fish)
  {
    fish.ApplyServerId(_nextId++);
    _fish.Add(CloneForResponse(fish));
    Save();
  }

  public TFish? Find<TFish>(int id) where TFish : Fish
  {
    return _fish.OfType<TFish>().FirstOrDefault(f => f.Id == id)?.Clone() as TFish;
  }

  public Fish? Update(int id, Fish updated)
  {
    var existing = _fish.FirstOrDefault(f => f.Id == id && f.TypeName == updated.TypeName);
    if (existing == null)
    {
      return null;
    }

    updated.ApplyServerId(id);
    existing.CopyFrom(updated);
    Save();
    return CloneForResponse(existing);
  }

  public bool Delete<TFish>(int id) where TFish : Fish
  {
    var existing = _fish.OfType<TFish>().FirstOrDefault(f => f.Id == id);
    if (existing == null)
    {
      return false;
    }

    _fish.Remove(existing);
    Save();
    return true;
  }

  private Fish CloneForResponse(Fish fish) => fish.Clone() as Fish ?? fish;

  private void Save()
  {
    var json = JsonSerializer.Serialize(_fish, JsonOptions);
    File.WriteAllText(_storagePath, json);
  }

  private IEnumerable<Fish> Load(string path)
  {
    if (!File.Exists(path))
    {
      yield break;
    }

    var json = File.ReadAllText(path);
    var document = JsonDocument.Parse(json);
    foreach (var element in document.RootElement.EnumerateArray())
    {
      if (!element.TryGetProperty("typeName", out var typeNameElement))
      {
        continue;
      }

      var typeName = typeNameElement.GetString();
      var type = typeName switch
      {
        nameof(Carp) => typeof(Carp),
        nameof(Bream) => typeof(Bream),
        nameof(Mackerel) => typeof(Mackerel),
        nameof(Tuna) => typeof(Tuna),
        _ => null
      };

      if (type == null)
      {
        continue;
      }

      var fish = (Fish?)element.Deserialize(type, JsonOptions);
      if (fish != null)
      {
        yield return fish;
      }
    }
  }
}
