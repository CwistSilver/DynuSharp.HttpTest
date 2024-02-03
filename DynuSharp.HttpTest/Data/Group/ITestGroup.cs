using System.Text.Json.Serialization;

namespace DynuSharp.HttpTest.Data.Group;
public interface ITestGroup
{
    [JsonPropertyName("name")]
    string Name { get; }

    [JsonPropertyName("selectedItems")]
    List<string> SelectedItems { get; set; }

    [JsonIgnore]
    IReadOnlyList<string> GroupItems { get; }

    Task<bool> Prepare(IDynuClient dynuClient);
    Task Run(IDynuClient dynuClient, TestContainer container);
}
