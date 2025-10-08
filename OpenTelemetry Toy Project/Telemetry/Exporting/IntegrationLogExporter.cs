using OpenTelemetry;
using OpenTelemetry.Logs;

namespace OpenTelemetry_Toy_Project.Telemetry.Exporting;

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
