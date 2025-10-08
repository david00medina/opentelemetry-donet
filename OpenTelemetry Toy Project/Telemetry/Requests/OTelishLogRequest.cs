using System.Text.Json.Serialization;

namespace OpenTelemetry_Toy_Project.Telemetry.Requests;

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
