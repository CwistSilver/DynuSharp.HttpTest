using System.Text.Json;
using System.Text.Json.Serialization;

namespace DynuSharp.HttpTest.Utilities;
public static class GlobalJsonOptions
{
    private static JsonSerializerOptions? _options;
    public static JsonSerializerOptions Options
    {
        get
        {
            if (_options is not null)
                return _options;

            _options = new JsonSerializerOptions() { WriteIndented = true };
            _options.Converters.Add(new JsonStringEnumConverter());
            _options.Converters.Add(new ITestGroupConverter());

            return _options;
        }
    }
}
