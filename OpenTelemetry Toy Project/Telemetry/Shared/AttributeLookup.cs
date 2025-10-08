using System.Globalization;
using System.Text.Json;

namespace OpenTelemetry_Toy_Project.Telemetry.Shared;

internal sealed class AttributeLookup
{
    private readonly Dictionary<string, string?> _values;

    private AttributeLookup(Dictionary<string, string?> values)
    {
        _values = values;
    }

    public static AttributeLookup From(
        IReadOnlyList<KeyValuePair<string, object>>? attributes,
        IReadOnlyList<KeyValuePair<string, object?>>? stateValues)
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
            if (!string.IsNullOrEmpty(key)
                && _values.TryGetValue(key, out var value)
                && int.TryParse(value, out var intValue))
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
            if (!string.IsNullOrEmpty(key)
                && _values.TryGetValue(key, out var value)
                && long.TryParse(value, out var longValue))
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
            if (!string.IsNullOrEmpty(key)
                && _values.TryGetValue(key, out var value)
                && double.TryParse(value, out var doubleValue))
            {
                return doubleValue;
            }
        }

        return null;
    }

    public static string? Serialize(
        IReadOnlyList<KeyValuePair<string, object>>? attributes,
        IReadOnlyList<KeyValuePair<string, object?>>? stateValues)
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
            IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
            _ => value.ToString()
        };
    }
}
