using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Duz_vadim_project;
using OpenApiValidator;

namespace UI.Services;

/// <summary>
/// Клиент для взаимодействия с сервером рыб с валидацией OpenAPI.
/// </summary>
public class FishApiClient
{
  private readonly HttpClient _httpClient;
  private readonly OpenApiSchemaValidator _validator;
  private readonly JsonSerializerOptions _serializerOptions = new(JsonSerializerDefaults.Web)
  {
    WriteIndented = false
  };

  /// <summary>
  /// Выполнять ли проверку входящих ответов по схеме OpenAPI.
  /// </summary>
  public bool ValidateResponses { get; set; } = true;

  /// <summary>
  /// Создаёт новый экземпляр клиента.
  /// </summary>
  /// <param name="httpClient">HTTP клиент.</param>
  /// <param name="schemaFilePath">Путь к файлу схемы OpenAPI.</param>
  public FishApiClient(HttpClient httpClient, string schemaFilePath)
  {
    _httpClient = httpClient;
    _validator = new OpenApiSchemaValidator(schemaFilePath);
  }

  /// <summary>
  /// Получает все коллекции рыб.
  /// </summary>
  public async Task<FishCollections?> GetCollectionsAsync(CancellationToken cancellationToken = default)
  {
    var (response, _) = await SendAsync<FishCollections>("getFishLists", HttpMethod.Get, "/list", null, cancellationToken, expectedStatuses: new[] { HttpStatusCode.OK });
    return response;
  }

  /// <summary>
  /// Создаёт карпа на сервере.
  /// </summary>
  public async Task<Carp?> CreateCarpAsync(Carp carp, CancellationToken cancellationToken = default)
  {
    var (response, _) = await SendAsync<Carp>("createCarp", HttpMethod.Post, "/Carp", carp, cancellationToken, expectedStatuses: new[] { HttpStatusCode.Created });
    return response;
  }

  /// <summary>
  /// Создаёт скумбрию на сервере.
  /// </summary>
  public async Task<Mackerel?> CreateMackerelAsync(Mackerel mackerel, CancellationToken cancellationToken = default)
  {
    var (response, _) = await SendAsync<Mackerel>("createMackerel", HttpMethod.Post, "/Mackerel", mackerel, cancellationToken, expectedStatuses: new[] { HttpStatusCode.Created });
    return response;
  }

  /// <summary>
  /// Обновляет данные карпа.
  /// </summary>
  public async Task<Carp?> UpdateCarpAsync(Carp carp, CancellationToken cancellationToken = default)
  {
    var (response, status) = await SendAsync<Carp>(
      "updateCarp",
      HttpMethod.Put,
      $"/Carp/{carp.Id}",
      carp,
      cancellationToken,
      expectedStatuses: new[]
      {
        HttpStatusCode.OK,
        HttpStatusCode.NotFound
      },
      "/Carp/{id}");
    return status == HttpStatusCode.OK ? response : null;
  }

  /// <summary>
  /// Обновляет данные скумбрии.
  /// </summary>
  public async Task<Mackerel?> UpdateMackerelAsync(Mackerel mackerel, CancellationToken cancellationToken = default)
  {
    var (response, status) = await SendAsync<Mackerel>(
      "updateMackerel",
      HttpMethod.Put,
      $"/Mackerel/{mackerel.Id}",
      mackerel,
      cancellationToken,
      expectedStatuses: new[]
      {
        HttpStatusCode.OK,
        HttpStatusCode.NotFound
      },
      "/Mackerel/{id}");
    return status == HttpStatusCode.OK ? response : null;
  }

  /// <summary>
  /// Удаляет карпа по идентификатору.
  /// </summary>
  public Task<bool> DeleteCarpAsync(Carp carp, CancellationToken cancellationToken = default)
  {
    return DeleteAsync("deleteCarp", $"/Carp/{carp.Id}", cancellationToken, "/Carp/{id}");
  }

  /// <summary>
  /// Удаляет скумбрию по идентификатору.
  /// </summary>
  public Task<bool> DeleteMackerelAsync(Mackerel mackerel, CancellationToken cancellationToken = default)
  {
    return DeleteAsync("deleteMackerel", $"/Mackerel/{mackerel.Id}", cancellationToken, "/Mackerel/{id}");
  }

  private async Task<bool> DeleteAsync(string operationId, string path, CancellationToken cancellationToken, string? templatePath = null)
  {
    var (_, status) = await SendAsync<ServerMessage>(
      operationId,
      HttpMethod.Delete,
      path,
      null,
      cancellationToken,
      expectedStatuses: new[]
      {
        HttpStatusCode.OK,
        HttpStatusCode.NotFound
      },
      templatePath);
    return status == HttpStatusCode.OK;
  }

  private async Task<(TResponse? Response, HttpStatusCode Status)> SendAsync<TResponse>(
    string operationId,
    HttpMethod method,
    string path,
    object? payload,
    CancellationToken cancellationToken,
    HttpStatusCode[]? expectedStatuses = null,
    string? templatePath = null)
  {
    var allowedStatuses = expectedStatuses is { Length: > 0 } ? expectedStatuses : new[] { HttpStatusCode.OK };
    var validationPath = templatePath ?? path;

    string? serializedPayload = null;
    if (payload != null)
    {
      serializedPayload = JsonSerializer.Serialize(payload, _serializerOptions);
      EnsureValid(_validator.ValidateRequest(operationId, method.Method, validationPath, serializedPayload), operationId, validationPath, isResponse: false);
    }

    using var request = new HttpRequestMessage(method, path);
    request.Headers.Accept.Clear();
    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

    if (serializedPayload != null)
    {
      request.Content = new StringContent(serializedPayload, Encoding.UTF8, "application/json");
    }

    var response = await _httpClient.SendAsync(request, cancellationToken);
    var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

    if (ValidateResponses)
    {
      EnsureValid(_validator.ValidateResponse(operationId, method.Method, validationPath, responseBody, ((int)response.StatusCode).ToString()), operationId, validationPath, isResponse: true);
    }

    if (!allowedStatuses.Contains(response.StatusCode))
    {
      if (TryBuildErrorMessage(responseBody, out var errorMessage))
      {
        throw new InvalidOperationException(errorMessage);
      }

      throw new InvalidOperationException($"Ожидался статус [{string.Join(", ", allowedStatuses)}], получено {response.StatusCode}: {responseBody}");
    }

    if (string.IsNullOrWhiteSpace(responseBody))
    {
      return (default, response.StatusCode);
    }

    var deserialized = JsonSerializer.Deserialize<TResponse>(responseBody, _serializerOptions);
    return (deserialized, response.StatusCode);
  }

  private static void EnsureValid(ValidationResult result, string operationId, string path, bool isResponse)
  {
    if (!result.IsValid)
    {
      var direction = isResponse ? "response" : "request";
      throw new InvalidOperationException($"OpenAPI validation failed for {direction} {operationId} ({path}): {result.ErrorMessage}");
    }
  }

  private bool TryBuildErrorMessage(string responseBody, out string? message)
  {
    message = null;
    if (string.IsNullOrWhiteSpace(responseBody))
    {
      return false;
    }

    try
    {
      var error = JsonSerializer.Deserialize<ErrorResponse>(responseBody, _serializerOptions);
      if (error == null)
      {
        return false;
      }

      var details = error.Errors is { Length: > 0 }
        ? $"{error.Description} ({string.Join("; ", error.Errors)})"
        : error.Description;

      message = string.IsNullOrWhiteSpace(details) ? null : details;
      return !string.IsNullOrWhiteSpace(message);
    }
    catch (JsonException)
    {
      return false;
    }
  }
}

/// <summary>
/// Сообщение сервера.
/// </summary>
/// <param name="Message">Текст сообщения.</param>
public record ServerMessage(string? Message);

/// <summary>
/// Ошибка, возвращаемая сервером.
/// </summary>
/// <param name="Code">Код ошибки.</param>
/// <param name="Description">Описание ошибки.</param>
/// <param name="Errors">Дополнительные детали.</param>
public record ErrorResponse(string? Code, string? Description, string[]? Errors);
