using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using Duz_vadim_project;
using OpenApiValidator;
using Server;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<FishRepository>();

var app = builder.Build();

var serializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
{
  WriteIndented = false
};
var schemaPath = Path.Combine(AppContext.BaseDirectory, "openapi.yaml");
var validator = new OpenApiSchemaValidator(schemaPath);

app.MapGet("/list", async (FishRepository repository) =>
  {
    var collections = await repository.GetCollectionsAsync();
    return ValidateResponse(validator, serializerOptions, "getFishLists", HttpMethod.Get, "/list", collections, HttpStatusCode.OK, () => Results.Ok(collections));
  })
  .WithName("getFishLists");

app.MapPost("/Carp", async (Carp carp, FishRepository repository) =>
  {
    if (!TryValidateFish(carp, out var validationErrors))
    {
      return ValidationProblem(validationErrors.Select(result => result.ErrorMessage ?? "Ошибка валидации данных"));
    }

    if (!TryValidateRequest(validator, serializerOptions, "createCarp", HttpMethod.Post, "/Carp", carp, out var requestError))
    {
      return requestError;
    }

    var created = await repository.AddCarpAsync(carp);
    return ValidateResponse(validator, serializerOptions, "createCarp", HttpMethod.Post, "/Carp", created, HttpStatusCode.Created, () => Results.Created($"/Carp/{created.Id}", created));
  })
  .WithName("createCarp");

app.MapGet("/Carp/{id:int}", async (int id, FishRepository repository) =>
  {
    var carp = await repository.GetCarpAsync(id);
    if (carp is null)
    {
      var notFound = new { message = "Объект не найден" };
      return ValidateResponse(validator, serializerOptions, "getCarpById", HttpMethod.Get, "/Carp/{id}", notFound, HttpStatusCode.NotFound, () => Results.NotFound(notFound));
    }

    return ValidateResponse(validator, serializerOptions, "getCarpById", HttpMethod.Get, "/Carp/{id}", carp, HttpStatusCode.OK, () => Results.Ok(carp));
  })
  .WithName("getCarpById");

app.MapPut("/Carp/{id:int}", async (int id, Carp carp, FishRepository repository) =>
  {
    carp.Id = id;
    if (!TryValidateFish(carp, out var validationErrors))
    {
      return ValidationProblem(validationErrors.Select(result => result.ErrorMessage ?? "Ошибка валидации данных"));
    }

    if (!TryValidateRequest(validator, serializerOptions, "updateCarp", HttpMethod.Put, "/Carp/{id}", carp, out var requestError))
    {
      return requestError;
    }

    var updated = await repository.UpdateCarpAsync(id, carp);
    if (updated is null)
    {
      var notFound = new { message = "Объект не найден" };
      return ValidateResponse(validator, serializerOptions, "updateCarp", HttpMethod.Put, "/Carp/{id}", notFound, HttpStatusCode.NotFound, () => Results.NotFound(notFound));
    }

    return ValidateResponse(validator, serializerOptions, "updateCarp", HttpMethod.Put, "/Carp/{id}", updated, HttpStatusCode.OK, () => Results.Ok(updated));
  })
  .WithName("updateCarp");

app.MapDelete("/Carp/{id:int}", async (int id, FishRepository repository) =>
  {
    var removed = await repository.DeleteCarpAsync(id);
    if (removed)
    {
      var ok = new { message = "Удалено" };
      return ValidateResponse(validator, serializerOptions, "deleteCarp", HttpMethod.Delete, "/Carp/{id}", ok, HttpStatusCode.OK, () => Results.Ok(ok));
    }

    var notFound = new { message = "Объект не найден" };
    return ValidateResponse(validator, serializerOptions, "deleteCarp", HttpMethod.Delete, "/Carp/{id}", notFound, HttpStatusCode.NotFound, () => Results.NotFound(notFound));
  })
  .WithName("deleteCarp");

app.MapPost("/Mackerel", async (Mackerel mackerel, FishRepository repository) =>
  {
    if (!TryValidateFish(mackerel, out var validationErrors))
    {
      return ValidationProblem(validationErrors.Select(result => result.ErrorMessage ?? "Ошибка валидации данных"));
    }

    if (!TryValidateRequest(validator, serializerOptions, "createMackerel", HttpMethod.Post, "/Mackerel", mackerel, out var requestError))
    {
      return requestError;
    }

    var created = await repository.AddMackerelAsync(mackerel);
    return ValidateResponse(validator, serializerOptions, "createMackerel", HttpMethod.Post, "/Mackerel", created, HttpStatusCode.Created, () => Results.Created($"/Mackerel/{created.Id}", created));
  })
  .WithName("createMackerel");

app.MapGet("/Mackerel/{id:int}", async (int id, FishRepository repository) =>
  {
    var mackerel = await repository.GetMackerelAsync(id);
    if (mackerel is null)
    {
      var notFound = new { message = "Объект не найден" };
      return ValidateResponse(validator, serializerOptions, "getMackerelById", HttpMethod.Get, "/Mackerel/{id}", notFound, HttpStatusCode.NotFound, () => Results.NotFound(notFound));
    }

    return ValidateResponse(validator, serializerOptions, "getMackerelById", HttpMethod.Get, "/Mackerel/{id}", mackerel, HttpStatusCode.OK, () => Results.Ok(mackerel));
  })
  .WithName("getMackerelById");

app.MapPut("/Mackerel/{id:int}", async (int id, Mackerel mackerel, FishRepository repository) =>
  {
    mackerel.Id = id;
    if (!TryValidateFish(mackerel, out var validationErrors))
    {
      return ValidationProblem(validationErrors.Select(result => result.ErrorMessage ?? "Ошибка валидации данных"));
    }

    if (!TryValidateRequest(validator, serializerOptions, "updateMackerel", HttpMethod.Put, "/Mackerel/{id}", mackerel, out var requestError))
    {
      return requestError;
    }

    var updated = await repository.UpdateMackerelAsync(id, mackerel);
    if (updated is null)
    {
      var notFound = new { message = "Объект не найден" };
      return ValidateResponse(validator, serializerOptions, "updateMackerel", HttpMethod.Put, "/Mackerel/{id}", notFound, HttpStatusCode.NotFound, () => Results.NotFound(notFound));
    }

    return ValidateResponse(validator, serializerOptions, "updateMackerel", HttpMethod.Put, "/Mackerel/{id}", updated, HttpStatusCode.OK, () => Results.Ok(updated));
  })
  .WithName("updateMackerel");

app.MapDelete("/Mackerel/{id:int}", async (int id, FishRepository repository) =>
  {
    var removed = await repository.DeleteMackerelAsync(id);
    if (removed)
    {
      var ok = new { message = "Удалено" };
      return ValidateResponse(validator, serializerOptions, "deleteMackerel", HttpMethod.Delete, "/Mackerel/{id}", ok, HttpStatusCode.OK, () => Results.Ok(ok));
    }

    var notFound = new { message = "Объект не найден" };
    return ValidateResponse(validator, serializerOptions, "deleteMackerel", HttpMethod.Delete, "/Mackerel/{id}", notFound, HttpStatusCode.NotFound, () => Results.NotFound(notFound));
  })
  .WithName("deleteMackerel");

app.Run();

static bool TryValidateFish(Fish fish, out List<ValidationResult> validationErrors)
{
  var validationContext = new ValidationContext(fish);
  validationErrors = new List<ValidationResult>();
  return Validator.TryValidateObject(fish, validationContext, validationErrors, true);
}

static bool TryValidateRequest<T>(OpenApiSchemaValidator validator, JsonSerializerOptions serializerOptions, string operationId, HttpMethod method, string path, T payload, out IResult? problemResult)
{
  var json = JsonSerializer.Serialize(payload, serializerOptions);
  var validation = validator.ValidateRequest(operationId, method.Method, path, json);
  if (validation.IsValid)
  {
    problemResult = null;
    return true;
  }

  problemResult = ValidationProblem(new[] { validation.ErrorMessage ?? "Ошибка валидации OpenAPI" });
  return false;
}

static IResult ValidateResponse<T>(OpenApiSchemaValidator validator, JsonSerializerOptions serializerOptions, string operationId, HttpMethod method, string path, T payload, HttpStatusCode statusCode, Func<IResult> onSuccess)
{
  var json = JsonSerializer.Serialize(payload, serializerOptions);
  var validation = validator.ValidateResponse(operationId, method.Method, path, json, ((int)statusCode).ToString());
  return validation.IsValid
    ? onSuccess()
    : Results.StatusCode(StatusCodes.Status500InternalServerError, new
    {
      message = "Ответ не соответствует схеме OpenAPI",
      errors = new[] { validation.ErrorMessage ?? "Ошибка валидации OpenAPI" }
    });
}

static IResult ValidationProblem(IEnumerable<string> validationErrors)
{
  return Results.BadRequest(new
  {
    message = "Данные не прошли валидацию",
    errors = validationErrors
  });
}
