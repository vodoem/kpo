using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Duz_vadim_project;
using OpenApiValidator;
using Server;
using ValidationResult = System.ComponentModel.DataAnnotations.ValidationResult;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<FishRepository>();

var app = builder.Build();

var serializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
{
  WriteIndented = false
};
var schemaPath = Path.Combine(AppContext.BaseDirectory, "openapi.yaml");
var validator = new OpenApiSchemaValidator(schemaPath);

app.Use(async (context, next) =>
{
  context.Request.EnableBuffering();
  string? body = null;
  if (context.Request.ContentLength is > 0 && context.Request.Body.CanRead)
  {
    using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true);
    body = await reader.ReadToEndAsync();
    context.Request.Body.Position = 0;
  }

  var requestInfo = $"Request {context.Request.Method} {context.Request.Path}";
  if (!string.IsNullOrWhiteSpace(body))
  {
    requestInfo += $" Body: {body}";
  }

  app.Logger.LogInformation(requestInfo);
  await next();
  app.Logger.LogInformation("Response {StatusCode} for {Method} {Path}", context.Response.StatusCode, context.Request.Method, context.Request.Path);
});

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
      return BuildErrorResponse(validator, serializerOptions, "createCarp", HttpMethod.Post, "/Carp", HttpStatusCode.BadRequest, "ValidationError", "Данные не прошли валидацию",
        validationErrors.Select(result => result.ErrorMessage ?? "Ошибка валидации данных"));
    }

    if (!TryValidateRequest(validator, serializerOptions, "createCarp", HttpMethod.Post, "/Carp", carp, out var requestError))
    {
      return requestError;
    }

    try
    {
      var created = await repository.AddCarpAsync(carp);
      return ValidateResponse(validator, serializerOptions, "createCarp", HttpMethod.Post, "/Carp", created, HttpStatusCode.Created, () => Results.Created($"/Carp/{created.Id}", created));
    }
    catch (DataFileCorruptedException ex)
    {
      return BuildErrorResponse(validator, serializerOptions, "createCarp", HttpMethod.Post, "/Carp", HttpStatusCode.InternalServerError, "DataFileCorrupted",
        "Файл данных поврежден. Невозможно сохранить изменения.", new[] { ex.Message });
    }
  })
  .WithName("createCarp");

app.MapGet("/Carp/{id:int}", async (int id, FishRepository repository) =>
  {
    var carp = await repository.GetCarpAsync(id);
    if (carp is null)
    {
      return BuildErrorResponse(validator, serializerOptions, "getCarpById", HttpMethod.Get, "/Carp/{id}", HttpStatusCode.NotFound, "NotFound", "Объект не найден");
    }

    return ValidateResponse(validator, serializerOptions, "getCarpById", HttpMethod.Get, "/Carp/{id}", carp, HttpStatusCode.OK, () => Results.Ok(carp));
  })
  .WithName("getCarpById");

app.MapPut("/Carp/{id:int}", async (int id, Carp carp, FishRepository repository) =>
  {
    carp.Id = id;
    if (!TryValidateFish(carp, out var validationErrors))
    {
      return BuildErrorResponse(validator, serializerOptions, "updateCarp", HttpMethod.Put, "/Carp/{id}", HttpStatusCode.BadRequest, "ValidationError", "Данные не прошли валидацию",
        validationErrors.Select(result => result.ErrorMessage ?? "Ошибка валидации данных"));
    }

    if (!TryValidateRequest(validator, serializerOptions, "updateCarp", HttpMethod.Put, "/Carp/{id}", carp, out var requestError))
    {
      return requestError;
    }

    try
    {
      var updated = await repository.UpdateCarpAsync(id, carp);
      if (updated is null)
      {
        return BuildErrorResponse(validator, serializerOptions, "updateCarp", HttpMethod.Put, "/Carp/{id}", HttpStatusCode.NotFound, "NotFound", "Объект не найден");
      }

      return ValidateResponse(validator, serializerOptions, "updateCarp", HttpMethod.Put, "/Carp/{id}", updated, HttpStatusCode.OK, () => Results.Ok(updated));
    }
    catch (DataFileCorruptedException ex)
    {
      return BuildErrorResponse(validator, serializerOptions, "updateCarp", HttpMethod.Put, "/Carp/{id}", HttpStatusCode.InternalServerError, "DataFileCorrupted",
        "Файл данных поврежден. Невозможно сохранить изменения.", new[] { ex.Message });
    }
  })
  .WithName("updateCarp");

app.MapDelete("/Carp/{id:int}", async (int id, FishRepository repository) =>
  {
    try
    {
      var removed = await repository.DeleteCarpAsync(id);
      if (removed)
      {
        var ok = new { message = "Удалено" };
        return ValidateResponse(validator, serializerOptions, "deleteCarp", HttpMethod.Delete, "/Carp/{id}", ok, HttpStatusCode.OK, () => Results.Ok(ok));
      }

      return BuildErrorResponse(validator, serializerOptions, "deleteCarp", HttpMethod.Delete, "/Carp/{id}", HttpStatusCode.NotFound, "NotFound", "Объект не найден");
    }
    catch (DataFileCorruptedException ex)
    {
      return BuildErrorResponse(validator, serializerOptions, "deleteCarp", HttpMethod.Delete, "/Carp/{id}", HttpStatusCode.InternalServerError, "DataFileCorrupted",
        "Файл данных поврежден. Невозможно сохранить изменения.", new[] { ex.Message });
    }
  })
  .WithName("deleteCarp");

app.MapPost("/Mackerel", async (Mackerel mackerel, FishRepository repository) =>
  {
    if (!TryValidateFish(mackerel, out var validationErrors))
    {
      return BuildErrorResponse(validator, serializerOptions, "createMackerel", HttpMethod.Post, "/Mackerel", HttpStatusCode.BadRequest, "ValidationError", "Данные не прошли валидацию",
        validationErrors.Select(result => result.ErrorMessage ?? "Ошибка валидации данных"));
    }

    if (!TryValidateRequest(validator, serializerOptions, "createMackerel", HttpMethod.Post, "/Mackerel", mackerel, out var requestError))
    {
      return requestError;
    }

    try
    {
      var created = await repository.AddMackerelAsync(mackerel);
      return ValidateResponse(validator, serializerOptions, "createMackerel", HttpMethod.Post, "/Mackerel", created, HttpStatusCode.Created, () => Results.Created($"/Mackerel/{created.Id}", created));
    }
    catch (DataFileCorruptedException ex)
    {
      return BuildErrorResponse(validator, serializerOptions, "createMackerel", HttpMethod.Post, "/Mackerel", HttpStatusCode.InternalServerError, "DataFileCorrupted",
        "Файл данных поврежден. Невозможно сохранить изменения.", new[] { ex.Message });
    }
  })
  .WithName("createMackerel");

app.MapGet("/Mackerel/{id:int}", async (int id, FishRepository repository) =>
  {
    var mackerel = await repository.GetMackerelAsync(id);
    if (mackerel is null)
    {
      return BuildErrorResponse(validator, serializerOptions, "getMackerelById", HttpMethod.Get, "/Mackerel/{id}", HttpStatusCode.NotFound, "NotFound", "Объект не найден");
    }

    return ValidateResponse(validator, serializerOptions, "getMackerelById", HttpMethod.Get, "/Mackerel/{id}", mackerel, HttpStatusCode.OK, () => Results.Ok(mackerel));
  })
  .WithName("getMackerelById");

app.MapPut("/Mackerel/{id:int}", async (int id, Mackerel mackerel, FishRepository repository) =>
  {
    mackerel.Id = id;
    if (!TryValidateFish(mackerel, out var validationErrors))
    {
      return BuildErrorResponse(validator, serializerOptions, "updateMackerel", HttpMethod.Put, "/Mackerel/{id}", HttpStatusCode.BadRequest, "ValidationError", "Данные не прошли валидацию",
        validationErrors.Select(result => result.ErrorMessage ?? "Ошибка валидации данных"));
    }

    if (!TryValidateRequest(validator, serializerOptions, "updateMackerel", HttpMethod.Put, "/Mackerel/{id}", mackerel, out var requestError))
    {
      return requestError;
    }

    try
    {
      var updated = await repository.UpdateMackerelAsync(id, mackerel);
      if (updated is null)
      {
        return BuildErrorResponse(validator, serializerOptions, "updateMackerel", HttpMethod.Put, "/Mackerel/{id}", HttpStatusCode.NotFound, "NotFound", "Объект не найден");
      }

      return ValidateResponse(validator, serializerOptions, "updateMackerel", HttpMethod.Put, "/Mackerel/{id}", updated, HttpStatusCode.OK, () => Results.Ok(updated));
    }
    catch (DataFileCorruptedException ex)
    {
      return BuildErrorResponse(validator, serializerOptions, "updateMackerel", HttpMethod.Put, "/Mackerel/{id}", HttpStatusCode.InternalServerError, "DataFileCorrupted",
        "Файл данных поврежден. Невозможно сохранить изменения.", new[] { ex.Message });
    }
  })
  .WithName("updateMackerel");

app.MapDelete("/Mackerel/{id:int}", async (int id, FishRepository repository) =>
  {
    try
    {
      var removed = await repository.DeleteMackerelAsync(id);
      if (removed)
      {
        var ok = new { message = "Удалено" };
        return ValidateResponse(validator, serializerOptions, "deleteMackerel", HttpMethod.Delete, "/Mackerel/{id}", ok, HttpStatusCode.OK, () => Results.Ok(ok));
      }

      return BuildErrorResponse(validator, serializerOptions, "deleteMackerel", HttpMethod.Delete, "/Mackerel/{id}", HttpStatusCode.NotFound, "NotFound", "Объект не найден");
    }
    catch (DataFileCorruptedException ex)
    {
      return BuildErrorResponse(validator, serializerOptions, "deleteMackerel", HttpMethod.Delete, "/Mackerel/{id}", HttpStatusCode.InternalServerError, "DataFileCorrupted",
        "Файл данных поврежден. Невозможно сохранить изменения.", new[] { ex.Message });
    }
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

  problemResult = BuildErrorResponse(validator, serializerOptions, operationId, method, path, HttpStatusCode.BadRequest, "ValidationError", "Запрос не соответствует схеме OpenAPI",
    new[] { validation.ErrorMessage ?? "Ошибка валидации OpenAPI" });
  return false;
}

static IResult ValidateResponse<T>(OpenApiSchemaValidator validator, JsonSerializerOptions serializerOptions, string operationId, HttpMethod method, string path, T payload, HttpStatusCode statusCode, Func<IResult> onSuccess)
{
  var json = JsonSerializer.Serialize(payload, serializerOptions);
  var validation = validator.ValidateResponse(operationId, method.Method, path, json, ((int)statusCode).ToString());
  return validation.IsValid
    ? onSuccess()
    : Results.Json(
    CreateErrorPayload("InternalServerError", "Произошла непредвиденная ошибка. Попробуйте позже",
      new[] { validation.ErrorMessage ?? "Ошибка валидации OpenAPI" }),
    statusCode: StatusCodes.Status500InternalServerError);
}

static IResult BuildErrorResponse(OpenApiSchemaValidator validator, JsonSerializerOptions serializerOptions, string operationId, HttpMethod method, string path, HttpStatusCode statusCode, string code, string description, IEnumerable<string>? errors = null)
{
  var payload = CreateErrorPayload(code, description, errors);
  return ValidateResponse(validator, serializerOptions, operationId, method, path, payload, statusCode, () => Results.Json(payload, statusCode: (int)statusCode));
}

static object CreateErrorPayload(string code, string description, IEnumerable<string>? errors = null)
{
  var errorList = errors?.Where(item => !string.IsNullOrWhiteSpace(item)).ToArray();
  return errorList is { Length: > 0 }
    ? new { code, description, errors = errorList }
    : new { code, description };
}
