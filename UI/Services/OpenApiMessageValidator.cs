using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;

namespace UI.Services;

/// <summary>
/// Простейшая валидация запросов и ответов на основе файла OpenAPI.
/// Проверяет наличие обязательных полей и соответствие базовых типов.
/// </summary>
public sealed class OpenApiMessageValidator
{
  private readonly OpenApiDocument _document;

  private OpenApiMessageValidator(OpenApiDocument document)
  {
    _document = document;
  }

  /// <summary>
  /// Создать валидатор, загрузив OpenAPI из файла.
  /// </summary>
  /// <param name="specPath">Путь к спецификации.</param>
  /// <exception cref="FileNotFoundException">Файл спецификации не найден.</exception>
  public static OpenApiMessageValidator FromFile(string specPath)
  {
    if (!File.Exists(specPath))
    {
      throw new FileNotFoundException("Не удалось найти файл OpenAPI", specPath);
    }

    using var stream = File.OpenRead(specPath);
    var reader = new OpenApiStreamReader();
    var document = reader.Read(stream, out var diagnostic);

    if (diagnostic.Errors.Count > 0)
    {
      var message = string.Join(Environment.NewLine, diagnostic.Errors.Select(error => $"{error.Pointer}: {error.Message}"));
      throw new InvalidOperationException($"OpenAPI содержит ошибки:\n{message}");
    }

    return new OpenApiMessageValidator(document);
  }

  /// <summary>
  /// Проверяет тело запроса на соответствие схеме из OpenAPI.
  /// </summary>
  public void ValidateRequest(string path, string method, JsonElement? body)
  {
    if (!_document.Paths.TryGetValue(path, out var pathItem))
    {
      throw new InvalidOperationException($"В OpenAPI отсутствует путь {path}");
    }

    if (!pathItem.Operations.TryGetValue(ParseMethod(method), out var operation))
    {
      throw new InvalidOperationException($"В OpenAPI отсутствует метод {method.ToUpperInvariant()} для пути {path}");
    }

    if (operation.RequestBody == null)
    {
      if (body.HasValue)
      {
        throw new InvalidOperationException($"Для {method.ToUpperInvariant()} {path} тело запроса не должно быть передано");
      }

      return;
    }

    if (!body.HasValue)
    {
      throw new InvalidOperationException($"Для {method.ToUpperInvariant()} {path} требуется тело запроса");
    }

    var schema = ResolveSchema(operation.RequestBody.Content["application/json"].Schema);
    ValidateElement(body.Value, schema, $"{method.ToUpperInvariant()} {path} (request)");
  }

  /// <summary>
  /// Проверяет тело ответа на соответствие схеме из OpenAPI.
  /// </summary>
  public void ValidateResponse(string path, string method, int statusCode, JsonElement? body)
  {
    if (!_document.Paths.TryGetValue(path, out var pathItem))
    {
      throw new InvalidOperationException($"В OpenAPI отсутствует путь {path}");
    }

    if (!pathItem.Operations.TryGetValue(ParseMethod(method), out var operation))
    {
      throw new InvalidOperationException($"В OpenAPI отсутствует метод {method.ToUpperInvariant()} для пути {path}");
    }

    var statusKey = statusCode.ToString(CultureInfo.InvariantCulture);
    if (!operation.Responses.TryGetValue(statusKey, out var response) &&
        !operation.Responses.TryGetValue("default", out response))
    {
      throw new InvalidOperationException($"В OpenAPI не описан ответ {statusCode} для {method.ToUpperInvariant()} {path}");
    }

    if (response.Content.Count == 0)
    {
      return;
    }

    if (!body.HasValue)
    {
      throw new InvalidOperationException($"Ответ {statusCode} для {method.ToUpperInvariant()} {path} должен содержать тело");
    }

    var schema = ResolveSchema(response.Content["application/json"].Schema);
    ValidateElement(body.Value, schema, $"{method.ToUpperInvariant()} {path} (response {statusCode})");
  }

  private static OperationType ParseMethod(string method) => method.ToUpperInvariant() switch
  {
    "GET" => OperationType.Get,
    "POST" => OperationType.Post,
    "PUT" => OperationType.Put,
    "DELETE" => OperationType.Delete,
    _ => throw new InvalidOperationException($"Метод {method} не поддерживается")
  };

  private OpenApiSchema ResolveSchema(OpenApiSchema schema)
  {
    if (schema.Reference?.Id is string id && _document.Components.Schemas.TryGetValue(id, out var referenced))
    {
      return referenced;
    }

    return schema;
  }

  private void ValidateElement(JsonElement element, OpenApiSchema schema, string context)
  {
    schema = ResolveSchema(schema);

    if (schema.OneOf.Count > 0)
    {
      ValidateOneOf(element, schema, context);
      return;
    }

    if (schema.AllOf.Count > 0)
    {
      foreach (var composed in schema.AllOf)
      {
        ValidateElement(element, composed, context);
      }

      return;
    }

    switch (schema.Type)
    {
      case "object":
        if (element.ValueKind != JsonValueKind.Object)
        {
          throw new InvalidOperationException($"{context}: ожидается объект JSON");
        }

        foreach (var required in schema.Required)
        {
          if (!element.TryGetProperty(required, out _))
          {
            throw new InvalidOperationException($"{context}: отсутствует обязательное свойство '{required}'");
          }
        }

        foreach (var property in schema.Properties)
        {
          if (element.TryGetProperty(property.Key, out var value))
          {
            ValidateElement(value, property.Value, $"{context} -> {property.Key}");
          }
        }

        break;

      case "array":
        if (element.ValueKind != JsonValueKind.Array)
        {
          throw new InvalidOperationException($"{context}: ожидается массив JSON");
        }

        foreach (var item in element.EnumerateArray())
        {
          ValidateElement(item, schema.Items, $"{context} -> элемент массива");
        }

        break;

      case "string":
        if (element.ValueKind != JsonValueKind.String)
        {
          throw new InvalidOperationException($"{context}: ожидается строка");
        }

        if (schema.Enum.Count > 0)
        {
          var value = element.GetString();
          var enumValues = schema.Enum
                                .Select(GetOpenApiStringValue)
                                .Where(parX => parX != null);
          if (!enumValues.Contains(value))
          {
            throw new InvalidOperationException($"{context}: недопустимое значение '{value}'");
          }
        }

        break;

      case "integer":
        if (element.ValueKind != JsonValueKind.Number || !element.TryGetInt64(out _))
        {
          throw new InvalidOperationException($"{context}: ожидается целое число");
        }

        break;

      case "number":
        if (element.ValueKind != JsonValueKind.Number)
        {
          throw new InvalidOperationException($"{context}: ожидается число");
        }

        break;

      case "boolean":
        if (element.ValueKind != JsonValueKind.True && element.ValueKind != JsonValueKind.False)
        {
          throw new InvalidOperationException($"{context}: ожидается логическое значение");
        }

        break;
    }
  }

  private void ValidateOneOf(JsonElement element, OpenApiSchema schema, string context)
  {
    if (element.ValueKind != JsonValueKind.Object)
    {
      throw new InvalidOperationException($"{context}: ожидается объект JSON для oneOf");
    }

    var discriminatorName = schema.Discriminator?.PropertyName ?? "typeName";
    if (!element.TryGetProperty(discriminatorName, out var discriminatorValue))
    {
      throw new InvalidOperationException($"{context}: отсутствует поле-дискриминатор '{discriminatorName}'");
    }

    var typeName = discriminatorValue.GetString();
    var target = ResolveByDiscriminator(schema, typeName);
    ValidateElement(element, target, context);
  }

  private OpenApiSchema ResolveByDiscriminator(OpenApiSchema schema, string? discriminator)
  {
    if (discriminator == null)
    {
      throw new InvalidOperationException("Дискриминатор не задан");
    }

    // Явная маппинг-таблица в схеме oneOf
    if (schema.Discriminator?.Mapping?.TryGetValue(discriminator, out var mappedRef) == true)
    {
      var mappedId = mappedRef.Split('/').Last();
      if (_document.Components.Schemas.TryGetValue(mappedId, out var mappedSchema))
      {
        return mappedSchema;
      }
    }

    foreach (var option in schema.OneOf)
    {
      var resolved = ResolveSchema(option);

      if (resolved.Properties.TryGetValue(schema.Discriminator?.PropertyName ?? "typeName", out var propertySchema) &&
          propertySchema.Enum
                         .Select(GetOpenApiStringValue)
                         .Where(parX => parX != null)
                         .Contains(discriminator))
      {
        return resolved;
      }

      if (resolved.Reference?.Id == discriminator)
      {
        return resolved;
      }
    }

    throw new InvalidOperationException($"Не удалось сопоставить тип '{discriminator}' со схемой oneOf");
  }

  private static string? GetOpenApiStringValue(IOpenApiAny? value)
  {
    return value switch
    {
      OpenApiString s => s.Value,
      _ => null
    };
  }
}
