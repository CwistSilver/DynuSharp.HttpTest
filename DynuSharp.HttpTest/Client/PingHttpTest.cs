using DynuSharp.HttpTest.Data;
using DynuSharp.HttpTest.Utilities;
using System.Text.Json;

namespace DynuSharp.HttpTest.Client;
internal class PingHttpTest : IHttpTest
{
    private const string TestName = "Ping";
    private readonly IDynuClient _client;
    private readonly HttpTestGroup _httpTestGroup;

    internal PingHttpTest(IDynuClient client, TestContainer container)
    {
        _client = client;

        _httpTestGroup = new HttpTestGroup(TestName);
        container.Results.Add(_httpTestGroup);
    }

    public async Task Run()
    {
        await PingWithBodyAsync();
        await PingWithQueryAsync();
    }

    private async Task PingWithBodyAsync()
    {
        string? queryResponse = null;

        await _httpTestGroup.AddTest(
            testName: $"Ping Dynu with body message",
            testFunction: async () => queryResponse = await _client.Ping.PingWithBodyAsync($"This is a unique ping check {Guid.NewGuid()}"),
            onSuccess: httpTestData => httpTestData.Data = JsonSerializer.SerializeToElement(queryResponse, GlobalJsonOptions.Options)
          );
    }

    private async Task PingWithQueryAsync()
    {
        string? queryResponse = null;

        await _httpTestGroup.AddTest(
            testName: $"Ping Dynu with query message",
            testFunction: async () => queryResponse = await _client.Ping.PingWithQueryAsync($"This is a unique ping check {Guid.NewGuid()}"),
            onSuccess: httpTestData => httpTestData.Data = JsonSerializer.SerializeToElement(queryResponse, GlobalJsonOptions.Options)
          );
    }
}
