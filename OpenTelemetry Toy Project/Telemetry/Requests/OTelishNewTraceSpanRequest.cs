using System.Text.Json.Serialization;

namespace OpenTelemetry_Toy_Project.Telemetry.Requests;

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
