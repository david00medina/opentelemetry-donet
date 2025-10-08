using OpenTelemetry.Logs;
using OpenTelemetry_Toy_Project.Telemetry.Requests;
using OpenTelemetry_Toy_Project.Telemetry.Shared;

namespace OpenTelemetry_Toy_Project.Telemetry.Exporting;

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
            SeverityText: record.LogLevel.ToString(),
            SeverityNumber: (int)record.LogLevel,
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
