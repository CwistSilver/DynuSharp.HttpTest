using DynuSharp.HttpTest.Data.Group;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DynuSharp.HttpTest.Utilities;
public sealed class ITestGroupConverter : JsonConverter<ITestGroup>
{
    private readonly ConcurrentDictionary<string, Type> _groupsDictionary = new();

    public ITestGroupConverter()
    {
        var assembly = Assembly.GetAssembly(typeof(ITestGroup));
        var types = assembly.GetTypes()
          .Where(type => typeof(ITestGroup).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract);


        foreach (var type in types)
        {
            var instance = (ITestGroup)Activator.CreateInstance(type)!;
            var typeValue = (string)type.GetProperty(nameof(ITestGroup.Name))!.GetValue(instance)!;
            _groupsDictionary.TryAdd(typeValue, type);
        }
    }

    public override ITestGroup Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Expected StartObject.");
        }

        string? name = null;
        List<string>? selectedItems = new List<string>();
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                break;

            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                var propertyName = reader.GetString();
                reader.Read();

                if (propertyName.ToLower() == nameof(ITestGroup.Name).ToLower())
                {
                    name = reader.GetString();
                }
                else if (propertyName.ToLower() == nameof(ITestGroup.SelectedItems).ToLower())
                {
                    var items = JsonSerializer.Deserialize<List<string>>(ref reader, options);
                    if (items is not null)
                        selectedItems.AddRange(items);
                }
                else
                {
                    reader.Skip();
                }
            }
        }

        if (name is null)
            throw new JsonException("Name property not found.");

        if (_groupsDictionary.TryGetValue(name, out var type))
        {
            var instance = (ITestGroup)Activator.CreateInstance(type)!;
            instance.SelectedItems = selectedItems;
            return instance;
        }

        throw new JsonException($"No test group found with the name {name}.");
    }

    public override void Write(Utf8JsonWriter writer, ITestGroup value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        var properties = typeof(ITestGroup).GetProperties();
        foreach (var prop in properties)
        {
            bool isJsonIgnore = prop.GetCustomAttribute<JsonIgnoreAttribute>() is not null;
            if (isJsonIgnore)
                continue;

            var propValue = prop.GetValue(value);

            var jsonPropertyNameAttribute = prop.GetCustomAttributes(typeof(JsonPropertyNameAttribute), false).FirstOrDefault() as JsonPropertyNameAttribute;
            string jsonPropertyName = jsonPropertyNameAttribute?.Name ?? prop.Name;

            writer.WritePropertyName(jsonPropertyName);
            JsonSerializer.Serialize(writer, propValue, prop.PropertyType, options);
        }

        writer.WriteEndObject();
    }
}
