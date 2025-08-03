using System.Text.Json;

namespace Postway.Core.Extensions;

public static class JsonExtension
{
    public static string ToJson(this object obj)
    {
        return JsonSerializer.Serialize(obj);
    }

    public static T? FromJson<T>(this string json) where T : class
    {
        T? result = null;
        try
        {
            result = JsonSerializer.Deserialize<T>(json);
        }
        catch (JsonException)
        {
            // Handle deserialization error if needed
        }
        return result;
    }
}
