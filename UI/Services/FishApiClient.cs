using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using Duz_vadim_project;

namespace UI.Services;

/// <summary>
/// Клиент для взаимодействия с сервером рыб через HTTP API.
/// </summary>
public sealed class FishApiClient
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _serializerOptions;
    private readonly OpenApiMessageValidator _validator;

    public FishApiClient(string baseAddress)
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(baseAddress, UriKind.Absolute)
        };

        _serializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        var openApiPath = Path.Combine(AppContext.BaseDirectory, "openapi.yaml");
        _validator = OpenApiMessageValidator.FromFile(openApiPath);
    }

    public async Task<IReadOnlyList<Fish>> GetAllAsync(CancellationToken cancellationToken)
    {
        _validator.ValidateRequest("/list", HttpMethod.Get.Method, null);

        var response = await _httpClient.GetAsync("/list", cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(content) ? "[]" : content);

        _validator.ValidateResponse("/list", HttpMethod.Get.Method, (int)response.StatusCode, document.RootElement);

        var fishList = JsonSerializer.Deserialize<List<Fish>>(document, _serializerOptions);
        return fishList ?? new List<Fish>();
    }

    public async Task<TFish> CreateAsync<TFish>(TFish fish, CancellationToken cancellationToken) where TFish : Fish
    {
        var payload = JsonSerializer.SerializeToElement(fish, _serializerOptions);
        var path = $"/{fish.TypeName}";

        _validator.ValidateRequest(path, HttpMethod.Post.Method, payload);

        var response = await _httpClient.PostAsJsonAsync(path, fish, _serializerOptions, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        using var document = JsonDocument.Parse(content);
        _validator.ValidateResponse(path, HttpMethod.Post.Method, (int)response.StatusCode, document.RootElement);

        return document.Deserialize<TFish>(_serializerOptions)!;
    }

    public async Task<TFish> UpdateAsync<TFish>(TFish fish, CancellationToken cancellationToken) where TFish : Fish
    {
        var payload = JsonSerializer.SerializeToElement(fish, _serializerOptions);
        var path = $"/{fish.TypeName}/{fish.Id}";
        var template = $"/{fish.TypeName}/{{id}}";

        _validator.ValidateRequest(template, HttpMethod.Put.Method, payload);

        var response = await _httpClient.PutAsJsonAsync(path, fish, _serializerOptions, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        using var document = JsonDocument.Parse(content);
        _validator.ValidateResponse(template, HttpMethod.Put.Method, (int)response.StatusCode, document.RootElement);

        return document.Deserialize<TFish>(_serializerOptions)!;
    }

    public async Task DeleteAsync(Fish fish, CancellationToken cancellationToken)
    {
        var path = $"/{fish.TypeName}/{fish.Id}";
        var template = $"/{fish.TypeName}/{{id}}";

        _validator.ValidateRequest(template, HttpMethod.Delete.Method, null);

        var response = await _httpClient.DeleteAsync(path, cancellationToken);
        response.EnsureSuccessStatusCode();

        _validator.ValidateResponse(template, HttpMethod.Delete.Method, (int)response.StatusCode, null);
    }
}
