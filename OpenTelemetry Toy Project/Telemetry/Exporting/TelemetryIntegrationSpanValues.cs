using System.Diagnostics;
using OpenTelemetry_Toy_Project.Telemetry.Requests;
using OpenTelemetry_Toy_Project.Telemetry.Shared;

namespace OpenTelemetry_Toy_Project.Telemetry.Exporting;

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
