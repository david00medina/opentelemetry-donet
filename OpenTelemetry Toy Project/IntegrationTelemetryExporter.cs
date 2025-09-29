using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using OpenTelemetry;
using OpenTelemetry.Logs;

namespace OpenTelemetry_Toy_Project;

internal sealed class IntegrationLogExporter : BaseExporter<LogRecord>
{
    public override ExportResult Export(in Batch<LogRecord> batch)
    {
        var success = true;

        foreach (var record in batch)
        {
            try
            {
                var values = TelemetryIntegrationLogValues.FromLogRecord(record);
                TelemetryExportClient.SendLog(values);
            }
            catch (Exception ex)
            {
                success = false;
                Console.Error.WriteLine($"Failed to export log record to telemetry service: {ex.Message}");
            }
        }

        return success ? ExportResult.Success : ExportResult.Failure;
    }
}

internal sealed class IntegrationSpanExporter : BaseExporter<Activity>
{
    public override ExportResult Export(in Batch<Activity> batch)
    {
        var success = true;

        foreach (var activity in batch)
        {
            try
            {
                var values = TelemetryIntegrationSpanValues.FromActivity(activity);
                TelemetryExportClient.SendTraceLifecycleRequests(values);
                TelemetryExportClient.NotifySpanCompletion(values);
            }
            catch (Exception ex)
            {
                success = false;
                Console.Error.WriteLine($"Failed to export span to telemetry service: {ex.Message}");
            }
        }

        return success ? ExportResult.Success : ExportResult.Failure;
    }
}

internal sealed record TelemetryIntegrationLogValues(
    string? TraceId,
    string? SpanId,
    string? SeverityText,
    int? SeverityNumber,
    string? Body,
    string? AttributesJson,
    string? EventName,
    string? CorrelationId,
    string? TenantId,
    long? RowsCount,
    string? HttpRequestMethod,
    string? HttpRequestUrl,
    string? HttpRequestRoute,
    string? HttpRequestDomain,
    string? HttpRequestScheme,
    int? HttpRequestPort,
    string? HttpRequestHeadersJson,
    string? HttpRequestBody,
    int? HttpResponseCode,
    double? HttpResponseTimeMs,
    string? HttpResponseMessage,
    string? HttpResponseHeadersJson)
{
    public static TelemetryIntegrationLogValues FromLogRecord(LogRecord record)
    {
        var lookup = AttributeLookup.From(record.Attributes, record.StateValues);
        var attributesJson = AttributeLookup.Serialize(record.Attributes, record.StateValues);

        return new TelemetryIntegrationLogValues(
            TraceId: record.TraceId != default ? record.TraceId.ToString() : null,
            SpanId: record.SpanId != default ? record.SpanId.ToString() : null,
            SeverityText: record.SeverityText,
            SeverityNumber: record.Severity.HasValue ? (int)record.Severity.Value : null,
            Body: record.FormattedMessage ?? record.Body?.ToString(),
            AttributesJson: attributesJson,
            EventName: record.EventId.Name,
            CorrelationId: lookup.GetString("correlation.id", "CorrelationId", "app.correlation_id"),
            TenantId: lookup.GetString("tenant.id", "TenantId"),
            RowsCount: lookup.GetLong("db.rows", "rows.count"),
            HttpRequestMethod: lookup.GetString("http.request.method", "http.method"),
            HttpRequestUrl: lookup.GetString("http.request.url", "url.full", "http.url"),
            HttpRequestRoute: lookup.GetString("http.request.route", "http.route"),
            HttpRequestDomain: lookup.GetString("http.request.domain", "server.address", "url.domain"),
            HttpRequestScheme: lookup.GetString("http.request.scheme", "url.scheme"),
            HttpRequestPort: lookup.GetInt("http.request.port", "server.port"),
            HttpRequestHeadersJson: lookup.GetString("http.request.headers", "request.headers"),
            HttpRequestBody: lookup.GetString("http.request.body", "request.body"),
            HttpResponseCode: lookup.GetInt("http.response.status_code", "http.status_code"),
            HttpResponseTimeMs: lookup.GetDouble("http.response.time_ms", "http.server.duration"),
            HttpResponseMessage: lookup.GetString("http.response.message", "response.message"),
            HttpResponseHeadersJson: lookup.GetString("http.response.headers", "response.headers"));
    }

    public OTelishLogRequest ToRequest() => new(
        TraceId,
        SpanId,
        SeverityText,
        SeverityNumber,
        Body,
        AttributesJson,
        EventName,
        CorrelationId,
        TenantId,
        RowsCount,
        HttpRequestMethod,
        HttpRequestUrl,
        HttpRequestRoute,
        HttpRequestDomain,
        HttpRequestScheme,
        HttpRequestPort,
        HttpRequestHeadersJson,
        HttpRequestBody,
        HttpResponseCode,
        HttpResponseTimeMs,
        HttpResponseMessage,
        HttpResponseHeadersJson);
}

internal sealed record TelemetryIntegrationSpanValues(
    string? TraceId,
    string? SpanId,
    string? ParentSpanId,
    string? ResourceName,
    string? Name,
    int StatusCode,
    string? StatusMessage,
    string? AttributesJson,
    string? CorrelationId,
    string? TenantId,
    string? TeamName,
    string? ResponsibleUser,
    int? MinimumTimeoutSeconds)
{
    public static TelemetryIntegrationSpanValues FromActivity(Activity activity)
    {
        var lookup = AttributeLookup.From(activity.TagObjects);
        var attributesJson = AttributeLookup.Serialize(activity.TagObjects);

        return new TelemetryIntegrationSpanValues(
            TraceId: activity.TraceId != default ? activity.TraceId.ToString() : null,
            SpanId: activity.SpanId.ToString(),
            ParentSpanId: activity.ParentSpanId != default ? activity.ParentSpanId.ToString() : null,
            ResourceName: activity.Source.Name,
            Name: activity.DisplayName,
            StatusCode: (int)activity.Status,
            StatusMessage: activity.StatusDescription,
            AttributesJson: attributesJson,
            CorrelationId: lookup.GetString("correlation.id", "CorrelationId", "app.correlation_id"),
            TenantId: lookup.GetString("tenant.id", "TenantId"),
            TeamName: lookup.GetString("team.name", "TeamName"),
            ResponsibleUser: lookup.GetString("user.responsible", "ResponsibleUser"),
            MinimumTimeoutSeconds: lookup.GetInt("timeout.min_seconds", "MinimumTimeoutSeconds"));
    }

    public OTelishSpanCompletionRequest ToCompletionRequest() => new(
        SpanId,
        StatusCode,
        StatusMessage);

    public OTelishNewTraceSpanRequest ToNewTraceRequest() => new(
        ParentSpanId,
        ResourceName,
        Name,
        StatusMessage,
        AttributesJson,
        CorrelationId,
        TenantId,
        TeamName,
        ResponsibleUser,
        MinimumTimeoutSeconds);

    public OTelishExistingTraceSpanRequest ToExistingTraceRequest()
    {
        if (TraceId is null)
        {
            throw new InvalidOperationException("Span TraceId is required to create a span for an existing trace.");
        }

        return new OTelishExistingTraceSpanRequest(
            TraceId,
            ParentSpanId,
            ResourceName,
            Name,
            StatusMessage,
            AttributesJson,
            CorrelationId,
            TenantId,
            TeamName,
            ResponsibleUser,
            MinimumTimeoutSeconds);
    }
}

internal static class TelemetryExportClient
{
    private static readonly HttpClient HttpClient = CreateHttpClient();

    public static void SendLog(TelemetryIntegrationLogValues values)
    {
        PostJson("api/integration/logs", values.ToRequest());
    }

    public static void SendTraceLifecycleRequests(TelemetryIntegrationSpanValues values)
    {
        if (string.IsNullOrEmpty(values.ParentSpanId))
        {
            PostJson("api/integration/trace", values.ToNewTraceRequest());
        }
        else
        {
            PutJson("api/integration/trace", values.ToExistingTraceRequest());
        }
    }

    public static void NotifySpanCompletion(TelemetryIntegrationSpanValues values)
    {
        PostJson("api/integration/span/completed", values.ToCompletionRequest());
    }

    private static HttpClient CreateHttpClient()
    {
        var client = new HttpClient
        {
            BaseAddress = new Uri("https://nifi-comon.com/"),
            Timeout = TimeSpan.FromSeconds(10),
        };

        return client;
    }

    private static void PostJson<T>(string relativeUrl, T payload)
    {
        using var response = HttpClient.PostAsJsonAsync(relativeUrl, payload).GetAwaiter().GetResult();
        response.EnsureSuccessStatusCode();
    }

    private static void PutJson<T>(string relativeUrl, T payload)
    {
        using var response = HttpClient.PutAsJsonAsync(relativeUrl, payload).GetAwaiter().GetResult();
        response.EnsureSuccessStatusCode();
    }
}

internal sealed record OTelishLogRequest(
    [property: JsonPropertyName("otel_trace_id")] string? TraceId,
    [property: JsonPropertyName("otel_span_id")] string? SpanId,
    [property: JsonPropertyName("otel_severity_text")] string? SeverityText,
    [property: JsonPropertyName("otel_severity_number")] int? SeverityNumber,
    [property: JsonPropertyName("otel_body")] string? Body,
    [property: JsonPropertyName("otel_attributes_json")] string? AttributesJson,
    [property: JsonPropertyName("otel_event_name")] string? EventName,
    [property: JsonPropertyName("otel_correlation_id")] string? CorrelationId,
    [property: JsonPropertyName("otel_tenant_id")] string? TenantId,
    [property: JsonPropertyName("otel_rows_count")] long? RowsCount,
    [property: JsonPropertyName("otel_http_request_method")] string? HttpRequestMethod,
    [property: JsonPropertyName("otel_http_request_url")] string? HttpRequestUrl,
    [property: JsonPropertyName("otel_http_request_route")] string? HttpRequestRoute,
    [property: JsonPropertyName("otel_http_request_domain")] string? HttpRequestDomain,
    [property: JsonPropertyName("otel_http_request_scheme")] string? HttpRequestScheme,
    [property: JsonPropertyName("otel_http_request_port")] int? HttpRequestPort,
    [property: JsonPropertyName("otel_http_request_headers_json")] string? HttpRequestHeadersJson,
    [property: JsonPropertyName("otel_http_request_body")] string? HttpRequestBody,
    [property: JsonPropertyName("otel_http_response_code")] int? HttpResponseCode,
    [property: JsonPropertyName("otel_http_response_time_ms")] double? HttpResponseTimeMs,
    [property: JsonPropertyName("otel_http_response_message")] string? HttpResponseMessage,
    [property: JsonPropertyName("otel_http_response_headers_json")] string? HttpResponseHeadersJson);

internal sealed record OTelishSpanCompletionRequest(
    [property: JsonPropertyName("otel_span_id")] string SpanId,
    [property: JsonPropertyName("otel_span_status_code")] int StatusCode,
    [property: JsonPropertyName("otel_span_status_message")] string? StatusMessage);

internal sealed record OTelishNewTraceSpanRequest(
    [property: JsonPropertyName("otel_parent_span_id")] string? ParentSpanId,
    [property: JsonPropertyName("otel_resource_name")] string? ResourceName,
    [property: JsonPropertyName("otel_name")] string? Name,
    [property: JsonPropertyName("otel_status_message")] string? StatusMessage,
    [property: JsonPropertyName("otel_attributes_json")] string? AttributesJson,
    [property: JsonPropertyName("otel_correlation_id")] string? CorrelationId,
    [property: JsonPropertyName("otel_tenant_id")] string? TenantId,
    [property: JsonPropertyName("otel_team_name")] string? TeamName,
    [property: JsonPropertyName("otel_responsible_user")] string? ResponsibleUser,
    [property: JsonPropertyName("otel_minimum_timeout_seconds")] int? MinimumTimeoutSeconds)
{
    [JsonPropertyName("otel_status_code")]
    public int StatusCode => 0;
}

internal sealed record OTelishExistingTraceSpanRequest(
    [property: JsonPropertyName("otel_trace_id")] string TraceId,
    [property: JsonPropertyName("otel_parent_span_id")] string? ParentSpanId,
    [property: JsonPropertyName("otel_resource_name")] string? ResourceName,
    [property: JsonPropertyName("otel_name")] string? Name,
    [property: JsonPropertyName("otel_status_message")] string? StatusMessage,
    [property: JsonPropertyName("otel_attributes_json")] string? AttributesJson,
    [property: JsonPropertyName("otel_correlation_id")] string? CorrelationId,
    [property: JsonPropertyName("otel_tenant_id")] string? TenantId,
    [property: JsonPropertyName("otel_team_name")] string? TeamName,
    [property: JsonPropertyName("otel_responsible_user")] string? ResponsibleUser,
    [property: JsonPropertyName("otel_minimum_timeout_seconds")] int? MinimumTimeoutSeconds)
{
    [JsonPropertyName("otel_status_code")]
    public int StatusCode => 0;
}

internal sealed class AttributeLookup
{
    private readonly Dictionary<string, string?> _values;

    private AttributeLookup(Dictionary<string, string?> values)
    {
        _values = values;
    }

    public static AttributeLookup From(IReadOnlyList<KeyValuePair<string, object>>? attributes, IReadOnlyList<KeyValuePair<string, object?>>? stateValues)
    {
        var values = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        if (attributes != null)
        {
            foreach (var pair in attributes)
            {
                values[pair.Key] = ConvertToString(pair.Value);
            }
        }

        if (stateValues != null)
        {
            foreach (var pair in stateValues)
            {
                if (!values.ContainsKey(pair.Key))
                {
                    values[pair.Key] = ConvertToString(pair.Value);
                }
            }
        }

        return new AttributeLookup(values);
    }

    public static AttributeLookup From(IEnumerable<KeyValuePair<string, object?>>? attributes)
    {
        var values = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        if (attributes != null)
        {
            foreach (var pair in attributes)
            {
                values[pair.Key] = ConvertToString(pair.Value);
            }
        }

        return new AttributeLookup(values);
    }

    public string? GetString(params string[] keys)
    {
        foreach (var key in keys)
        {
            if (!string.IsNullOrEmpty(key) && _values.TryGetValue(key, out var value))
            {
                return value;
            }
        }

        return null;
    }

    public int? GetInt(params string[] keys)
    {
        foreach (var key in keys)
        {
            if (!string.IsNullOrEmpty(key) && _values.TryGetValue(key, out var value) && int.TryParse(value, out var intValue))
            {
                return intValue;
            }
        }

        return null;
    }

    public long? GetLong(params string[] keys)
    {
        foreach (var key in keys)
        {
            if (!string.IsNullOrEmpty(key) && _values.TryGetValue(key, out var value) && long.TryParse(value, out var longValue))
            {
                return longValue;
            }
        }

        return null;
    }

    public double? GetDouble(params string[] keys)
    {
        foreach (var key in keys)
        {
            if (!string.IsNullOrEmpty(key) && _values.TryGetValue(key, out var value) && double.TryParse(value, out var doubleValue))
            {
                return doubleValue;
            }
        }

        return null;
    }

    public static string? Serialize(IReadOnlyList<KeyValuePair<string, object>>? attributes, IReadOnlyList<KeyValuePair<string, object?>>? stateValues)
    {
        var values = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

        if (attributes != null)
        {
            foreach (var pair in attributes)
            {
                values[pair.Key] = pair.Value;
            }
        }

        if (stateValues != null)
        {
            foreach (var pair in stateValues)
            {
                if (!values.ContainsKey(pair.Key))
                {
                    values[pair.Key] = pair.Value;
                }
            }
        }

        return Serialize(values);
    }

    public static string? Serialize(IEnumerable<KeyValuePair<string, object?>>? attributes)
    {
        if (attributes == null)
        {
            return null;
        }

        var values = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        foreach (var pair in attributes)
        {
            values[pair.Key] = pair.Value;
        }

        return Serialize(values);
    }

    private static string? Serialize(Dictionary<string, object?> values)
    {
        if (values.Count == 0)
        {
            return null;
        }

        var sanitized = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        foreach (var pair in values)
        {
            sanitized[pair.Key] = pair.Value switch
            {
                null => null,
                string => pair.Value,
                bool => pair.Value,
                int => pair.Value,
                long => pair.Value,
                double => pair.Value,
                float => pair.Value,
                { } other => other.ToString()
            };
        }

        return JsonSerializer.Serialize(sanitized);
    }

    private static string? ConvertToString(object? value)
    {
        if (value == null)
        {
            return null;
        }

        return value switch
        {
            string s => s,
            IFormattable formattable => formattable.ToString(null, System.Globalization.CultureInfo.InvariantCulture),
            _ => value.ToString()
        };
    }
}
