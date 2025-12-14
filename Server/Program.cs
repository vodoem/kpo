using System.ComponentModel.DataAnnotations;
using System.Linq;
using Duz_vadim_project;
using Server;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<FishRepository>();

var app = builder.Build();

app.MapGet("/list", async (FishRepository repository) => Results.Ok(await repository.GetCollectionsAsync()))
  .WithName("getFishLists");

app.MapPost("/Carp", async (Carp carp, FishRepository repository) =>
  {
    if (!TryValidateFish(carp, out var validationErrors))
    {
      return ValidationProblem(validationErrors);
    }

    var created = await repository.AddCarpAsync(carp);
    return Results.Created($"/Carp/{created.Id}", created);
  })
  .WithName("createCarp");

app.MapGet("/Carp/{id:int}", async (int id, FishRepository repository) =>
  {
    var carp = await repository.GetCarpAsync(id);
    return carp is null
      ? Results.NotFound(new { message = "Объект не найден" })
      : Results.Ok(carp);
  })
  .WithName("getCarpById");

app.MapPut("/Carp/{id:int}", async (int id, Carp carp, FishRepository repository) =>
  {
    carp.Id = id;
    if (!TryValidateFish(carp, out var validationErrors))
    {
      return ValidationProblem(validationErrors);
    }

    var updated = await repository.UpdateCarpAsync(id, carp);
    return updated is null
      ? Results.NotFound(new { message = "Объект не найден" })
      : Results.Ok(updated);
  })
  .WithName("updateCarp");

app.MapDelete("/Carp/{id:int}", async (int id, FishRepository repository) =>
  {
    var removed = await repository.DeleteCarpAsync(id);
    return removed
      ? Results.Ok(new { message = "Удалено" })
      : Results.NotFound(new { message = "Объект не найден" });
  })
  .WithName("deleteCarp");

app.MapPost("/Mackerel", async (Mackerel mackerel, FishRepository repository) =>
  {
    if (!TryValidateFish(mackerel, out var validationErrors))
    {
      return ValidationProblem(validationErrors);
    }

    var created = await repository.AddMackerelAsync(mackerel);
    return Results.Created($"/Mackerel/{created.Id}", created);
  })
  .WithName("createMackerel");

app.MapGet("/Mackerel/{id:int}", async (int id, FishRepository repository) =>
  {
    var mackerel = await repository.GetMackerelAsync(id);
    return mackerel is null
      ? Results.NotFound(new { message = "Объект не найден" })
      : Results.Ok(mackerel);
  })
  .WithName("getMackerelById");

app.MapPut("/Mackerel/{id:int}", async (int id, Mackerel mackerel, FishRepository repository) =>
  {
    mackerel.Id = id;
    if (!TryValidateFish(mackerel, out var validationErrors))
    {
      return ValidationProblem(validationErrors);
    }

    var updated = await repository.UpdateMackerelAsync(id, mackerel);
    return updated is null
      ? Results.NotFound(new { message = "Объект не найден" })
      : Results.Ok(updated);
  })
  .WithName("updateMackerel");

app.MapDelete("/Mackerel/{id:int}", async (int id, FishRepository repository) =>
  {
    var removed = await repository.DeleteMackerelAsync(id);
    return removed
      ? Results.Ok(new { message = "Удалено" })
      : Results.NotFound(new { message = "Объект не найден" });
  })
  .WithName("deleteMackerel");

app.Run();

static bool TryValidateFish(Fish fish, out List<ValidationResult> validationErrors)
{
  var validationContext = new ValidationContext(fish);
  validationErrors = new List<ValidationResult>();
  return Validator.TryValidateObject(fish, validationContext, validationErrors, true);
}

static IResult ValidationProblem(IEnumerable<ValidationResult> validationErrors)
{
  var messages = validationErrors.Select(result => result.ErrorMessage ?? "Ошибка валидации");
  return Results.BadRequest(new
  {
    message = "Данные не прошли валидацию",
    errors = messages
  });
}
