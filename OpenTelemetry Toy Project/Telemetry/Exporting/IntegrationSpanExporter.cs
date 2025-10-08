using System.Diagnostics;
using OpenTelemetry;

namespace OpenTelemetry_Toy_Project.Telemetry.Exporting;

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
