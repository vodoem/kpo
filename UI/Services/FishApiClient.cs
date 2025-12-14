using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
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
    var (response, _) = await SendAsync<FishCollections>("getFishLists", HttpMethod.Get, "/list", null, cancellationToken, HttpStatusCode.OK);
    return response;
  }

  /// <summary>
  /// Создаёт карпа на сервере.
  /// </summary>
  public async Task<Carp?> CreateCarpAsync(Carp carp, CancellationToken cancellationToken = default)
  {
    var (response, _) = await SendAsync<Carp>("createCarp", HttpMethod.Post, "/Carp", carp, cancellationToken, HttpStatusCode.Created);
    return response;
  }

  /// <summary>
  /// Создаёт скумбрию на сервере.
  /// </summary>
  public async Task<Mackerel?> CreateMackerelAsync(Mackerel mackerel, CancellationToken cancellationToken = default)
  {
    var (response, _) = await SendAsync<Mackerel>("createMackerel", HttpMethod.Post, "/Mackerel", mackerel, cancellationToken, HttpStatusCode.Created);
    return response;
  }

  /// <summary>
  /// Обновляет данные карпа.
  /// </summary>
  public async Task<Carp?> UpdateCarpAsync(Carp carp, CancellationToken cancellationToken = default)
  {
    var (response, status) = await SendAsync<Carp>("updateCarp", HttpMethod.Put, $"/Carp/{carp.Id}", carp, cancellationToken, HttpStatusCode.OK, HttpStatusCode.NotFound);
    return status == HttpStatusCode.OK ? response : null;
  }

  /// <summary>
  /// Обновляет данные скумбрии.
  /// </summary>
  public async Task<Mackerel?> UpdateMackerelAsync(Mackerel mackerel, CancellationToken cancellationToken = default)
  {
    var (response, status) = await SendAsync<Mackerel>("updateMackerel", HttpMethod.Put, $"/Mackerel/{mackerel.Id}", mackerel, cancellationToken, HttpStatusCode.OK, HttpStatusCode.NotFound);
    return status == HttpStatusCode.OK ? response : null;
  }

  /// <summary>
  /// Удаляет карпа по идентификатору.
  /// </summary>
  public Task<bool> DeleteCarpAsync(Carp carp, CancellationToken cancellationToken = default)
  {
    return DeleteAsync("deleteCarp", $"/Carp/{carp.Id}", cancellationToken);
  }

  /// <summary>
  /// Удаляет скумбрию по идентификатору.
  /// </summary>
  public Task<bool> DeleteMackerelAsync(Mackerel mackerel, CancellationToken cancellationToken = default)
  {
    return DeleteAsync("deleteMackerel", $"/Mackerel/{mackerel.Id}", cancellationToken);
  }

  private async Task<bool> DeleteAsync(string operationId, string path, CancellationToken cancellationToken)
  {
    var (_, status) = await SendAsync<ServerMessage>(operationId, HttpMethod.Delete, path, null, cancellationToken, HttpStatusCode.OK, HttpStatusCode.NotFound);
    return status == HttpStatusCode.OK;
  }

  private async Task<(TResponse? Response, HttpStatusCode Status)> SendAsync<TResponse>(
    string operationId,
    HttpMethod method,
    string path,
    object? payload,
    CancellationToken cancellationToken,
    params HttpStatusCode[] expectedStatuses)
  {
    var allowedStatuses = expectedStatuses.Length == 0 ? new[] { HttpStatusCode.OK } : expectedStatuses;

    string? serializedPayload = null;
    if (payload != null)
    {
      serializedPayload = JsonSerializer.Serialize(payload, _serializerOptions);
      EnsureValid(_validator.ValidateRequest(operationId, method.Method, path, serializedPayload), operationId, path, isResponse: false);
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

    EnsureValid(_validator.ValidateResponse(operationId, method.Method, path, responseBody, ((int)response.StatusCode).ToString()), operationId, path, isResponse: true);

    if (!allowedStatuses.Contains(response.StatusCode))
    {
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
}

/// <summary>
/// Сообщение сервера.
/// </summary>
/// <param name="Message">Текст сообщения.</param>
public record ServerMessage(string? Message);
