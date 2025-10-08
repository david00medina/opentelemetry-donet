using System.Text.Json.Serialization;

namespace OpenTelemetry_Toy_Project.Telemetry.Requests;

internal sealed record OTelishSpanCompletionRequest(
    [property: JsonPropertyName("otel_span_id")] string SpanId,
    [property: JsonPropertyName("otel_span_status_code")] int StatusCode,
    [property: JsonPropertyName("otel_span_status_message")] string? StatusMessage);
