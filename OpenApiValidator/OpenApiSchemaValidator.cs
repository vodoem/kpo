using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;

namespace OpenApiValidator;

/// <summary>
/// Валидатор запросов и ответов по схеме OpenAPI
/// </summary>
public class OpenApiSchemaValidator
{
    private readonly OpenApiDocument _openApiDocument;

    /// <summary>
    /// Конструктор с загрузкой схемы из файла
    /// </summary>
    /// <param name="schemaFilePath">Путь к файлу схемы OpenAPI</param>
    public OpenApiSchemaValidator(string schemaFilePath)
    {
        if (!File.Exists(schemaFilePath))
        {
            throw new FileNotFoundException($"OpenAPI schema file not found: {schemaFilePath}");
        }

        var schemaJson = File.ReadAllText(schemaFilePath);
        var reader = new OpenApiStringReader();
        _openApiDocument = reader.Read(schemaJson, out var diagnostic);

        if (diagnostic.Errors.Count > 0)
        {
            throw new InvalidOperationException($"OpenAPI schema validation errors: {string.Join(", ", diagnostic.Errors)}");
        }
    }

    /// <summary>
    /// Валидация входящего запроса
    /// </summary>
    /// <param name="operationId">Идентификатор операции</param>
    /// <param name="method">HTTP метод</param>
    /// <param name="path">Путь API</param>
    /// <param name="jsonPayload">JSON payload запроса</param>
    /// <returns>Результат валидации</returns>
    public ValidationResult ValidateRequest(string operationId, string method, string path, string jsonPayload)
    {
        try
        {
            var operation = FindOperation(operationId, method, path);
            if (operation?.RequestBody?.Content?.ContainsKey("application/json") == true)
            {
                var schema = operation.RequestBody.Content["application/json"].Schema;
                return ValidateAgainstSchema(jsonPayload, schema);
            }

            return ValidationResult.Success();
        }
        catch (Exception ex)
        {
            return ValidationResult.Error($"Validation error: {ex.Message}");
        }
    }

    /// <summary>
    /// Валидация исходящего ответа
    /// </summary>
    /// <param name="operationId">Идентификатор операции</param>
    /// <param name="method">HTTP метод</param>
    /// <param name="path">Путь API</param>
    /// <param name="jsonPayload">JSON payload ответа</param>
    /// <param name="statusCode">HTTP статус код</param>
    /// <returns>Результат валидации</returns>
    public ValidationResult ValidateResponse(string operationId, string method, string path, string jsonPayload, string statusCode = "200")
    {
        try
        {
            var operation = FindOperation(operationId, method, path);
            if (operation?.Responses?.ContainsKey(statusCode) == true)
            {
                var response = operation.Responses[statusCode];
                if (response.Content?.ContainsKey("application/json") == true)
                {
                    var schema = response.Content["application/json"].Schema;
                    return ValidateAgainstSchema(jsonPayload, schema);
                }
            }

            return ValidationResult.Success();
        }
        catch (Exception ex)
        {
            return ValidationResult.Error($"Validation error: {ex.Message}");
        }
    }

    private OpenApiOperation FindOperation(string operationId, string method, string path)
    {
        var resolvedPath = ResolvePath(path)
            ?? throw new InvalidOperationException($"Path {path} not found in OpenAPI schema");

        var pathItem = _openApiDocument.Paths[resolvedPath];
        var operationType = method.ToLower() switch
        {
            "get" => OperationType.Get,
            "post" => OperationType.Post,
            "put" => OperationType.Put,
            "delete" => OperationType.Delete,
            _ => throw new InvalidOperationException($"Unsupported HTTP method: {method}")
        };

        if (!pathItem.Operations.ContainsKey(operationType))
        {
            throw new InvalidOperationException($"Operation {method} not found for path {path}");
        }

        var operation = pathItem.Operations[operationType];

        if (operation.OperationId != operationId)
        {
            throw new InvalidOperationException($"Operation ID mismatch. Expected: {operationId}, Actual: {operation.OperationId}");
        }

        return operation;
    }

    private string? ResolvePath(string path)
    {
        if (_openApiDocument.Paths.ContainsKey(path))
        {
            return path;
        }

        var requestedSegments = path.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);

        foreach (var candidate in _openApiDocument.Paths.Keys)
        {
            var templateSegments = candidate.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (requestedSegments.Length != templateSegments.Length)
            {
                continue;
            }

            var matched = true;
            for (int i = 0; i < requestedSegments.Length; i++)
            {
                var templateSegment = templateSegments[i];
                if (templateSegment.StartsWith("{") && templateSegment.EndsWith("}"))
                {
                    continue;
                }

                if (!string.Equals(templateSegment, requestedSegments[i], StringComparison.Ordinal))
                {
                    matched = false;
                    break;
                }
            }

            if (matched)
            {
                return candidate;
            }
        }

        return null;
    }

    private ValidationResult ValidateAgainstSchema(string jsonPayload, OpenApiSchema schema)
    {
        try
        {
            using var document = JsonDocument.Parse(jsonPayload);
            var errors = new List<string>();

            ValidateJsonElement(document.RootElement, schema, errors, string.Empty);

            return errors.Any()
                ? ValidationResult.Error(string.Join("; ", errors))
                : ValidationResult.Success();
        }
        catch (JsonException ex)
        {
            return ValidationResult.Error($"Invalid JSON: {ex.Message}");
        }
    }

    private void ValidateJsonElement(JsonElement element, OpenApiSchema schema, List<string> errors, string path)
    {
        if (schema.Type != null && !IsTypeMatch(element.ValueKind, schema.Type))
        {
            errors.Add($"{path}: Expected type {schema.Type}, got {element.ValueKind}");
            return;
        }

        if (element.ValueKind == JsonValueKind.Object)
        {
            if (schema.Required != null)
            {
                foreach (var requiredField in schema.Required)
                {
                    if (!element.TryGetProperty(requiredField, out _))
                    {
                        errors.Add($"Missing required property: {AppendPath(path, requiredField)}");
                    }
                }
            }

            if (schema.Properties != null)
            {
                foreach (var property in schema.Properties)
                {
                    var propertyName = property.Key;
                    var propertySchema = property.Value;
                    var fullPath = AppendPath(path, propertyName);

                    if (element.TryGetProperty(propertyName, out var propertyValue))
                    {
                        ValidatePropertyValue(propertyValue, propertySchema, errors, fullPath);
                    }
                }
            }
        }

        if (element.ValueKind == JsonValueKind.Array && schema.Items != null)
        {
            var index = 0;
            foreach (var item in element.EnumerateArray())
            {
                ValidateJsonElement(item, schema.Items, errors, $"{path}[{index}]");
                index++;
            }
        }

        if (schema.OneOf != null && schema.OneOf.Count > 0)
        {
            var oneOfValid = false;
            foreach (var oneOfSchema in schema.OneOf)
            {
                var oneOfErrors = new List<string>();
                ValidateJsonElement(element, oneOfSchema, oneOfErrors, path);
                if (!oneOfErrors.Any())
                {
                    oneOfValid = true;
                    break;
                }
            }

            if (!oneOfValid)
            {
                errors.Add($"{path}: Does not match any of the expected schemas");
            }
        }
    }

    private void ValidatePropertyValue(JsonElement value, OpenApiSchema schema, List<string> errors, string path)
    {
        if (value.ValueKind == JsonValueKind.String && schema.Type == "string")
        {
            var stringValue = value.GetString() ?? string.Empty;
            if (schema.MinLength.HasValue && stringValue.Length < schema.MinLength.Value)
            {
                errors.Add($"{path}: String length ({stringValue.Length}) less than minimum ({schema.MinLength})");
            }

            if (schema.MaxLength.HasValue && stringValue.Length > schema.MaxLength.Value)
            {
                errors.Add($"{path}: String length ({stringValue.Length}) exceeds maximum ({schema.MaxLength})");
            }
        }
        else if (value.ValueKind == JsonValueKind.Number && (schema.Type == "number" || schema.Type == "integer"))
        {
            var numericValue = value.GetDecimal();
            if (schema.Minimum.HasValue && numericValue < schema.Minimum.Value)
            {
                errors.Add($"{path}: Value ({numericValue}) less than minimum ({schema.Minimum})");
            }

            if (schema.Maximum.HasValue && numericValue > schema.Maximum.Value)
            {
                errors.Add($"{path}: Value ({numericValue}) exceeds maximum ({schema.Maximum})");
            }
        }
        else if ((value.ValueKind == JsonValueKind.True || value.ValueKind == JsonValueKind.False) && schema.Type == "boolean")
        {
        }
        else if (value.ValueKind == JsonValueKind.Object || value.ValueKind == JsonValueKind.Array)
        {
            ValidateJsonElement(value, schema, errors, path);
        }
        else if (schema.Enum != null)
        {
            var stringValue = value.ValueKind == JsonValueKind.String ? value.GetString() : value.ToString();
            if (!schema.Enum.Any(e => e.ToString() == stringValue))
            {
                errors.Add($"{path}: Value '{stringValue}' is not in allowed values: {string.Join(", ", schema.Enum)}");
            }
        }
    }

    private static string AppendPath(string path, string node)
    {
        return string.IsNullOrEmpty(path) ? node : $"{path}.{node}";
    }

    private static bool IsTypeMatch(JsonValueKind valueKind, string schemaType)
    {
        return schemaType switch
        {
            "string" => valueKind == JsonValueKind.String,
            "number" => valueKind == JsonValueKind.Number,
            "integer" => valueKind == JsonValueKind.Number,
            "boolean" => valueKind == JsonValueKind.True || valueKind == JsonValueKind.False,
            "object" => valueKind == JsonValueKind.Object,
            "array" => valueKind == JsonValueKind.Array,
            _ => true
        };
    }
}

/// <summary>
/// Результат валидации
/// </summary>
public class ValidationResult
{
    /// <summary>
    /// Флаг успешности валидации
    /// </summary>
    public bool IsValid { get; }

    /// <summary>
    /// Сообщение об ошибке (если есть)
    /// </summary>
    public string? ErrorMessage { get; }

    private ValidationResult(bool isValid, string? errorMessage)
    {
        IsValid = isValid;
        ErrorMessage = errorMessage;
    }

    /// <summary>
    /// Успешный результат валидации
    /// </summary>
    public static ValidationResult Success() => new(true, null);

    /// <summary>
    /// Результат валидации с ошибкой
    /// </summary>
    public static ValidationResult Error(string message) => new(false, message);
}
