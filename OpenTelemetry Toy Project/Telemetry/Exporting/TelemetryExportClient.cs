using System.Net.Http.Json;

namespace OpenTelemetry_Toy_Project.Telemetry.Exporting;

internal static class TelemetryExportClient
{
    private static readonly HttpClient HttpClient = CreateHttpClient();

    public static void SendLog(TelemetryIntegrationLogValues values)
    {
        PostJson("api/integrations/logs", values.ToRequest());
    }

    public static void SendTraceLifecycleRequests(TelemetryIntegrationSpanValues values)
    {
        if (string.IsNullOrEmpty(values.ParentSpanId))
        {
            PostJson("api/integrations/trace", values.ToNewTraceRequest());
        }
        else
        {
            PutJson("api/integrations/trace", values.ToExistingTraceRequest());
        }
    }

    public static void NotifySpanCompletion(TelemetryIntegrationSpanValues values)
    {
        PostJson("api/integrations/span/completed", values.ToCompletionRequest());
    }

    private static HttpClient CreateHttpClient()
    {
        var client = new HttpClient
        {
            BaseAddress = new Uri("https://bc095d3c-1aa5-4014-9e32-604ff38e3ce0.mock.pstmn.io/"),
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
