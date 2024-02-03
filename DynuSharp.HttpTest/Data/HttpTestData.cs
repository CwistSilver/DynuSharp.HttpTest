using System.Text.Json.Serialization;

namespace DynuSharp.HttpTest.Data;
public class HttpTestData
{
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("result")]
    public HttpTestResult Result { get; set; }

    [JsonPropertyName("data")]
    public object? Data { get; set; }
}
