using DynuSharp.Data.Limit;
using DynuSharp.HttpTest.Data;
using DynuSharp.HttpTest.Utilities;
using System.Text.Json;

namespace DynuSharp.HttpTest.Client.Monitor;
internal class LimitHttpTest : IHttpTest
{
    private const string TestName = "Monitor Limit";
    private readonly IDynuClient _client;
    private readonly HttpTestGroup _httpTestGroup;
    internal LimitHttpTest(IDynuClient client, TestContainer container)
    {
        _client = client;

        _httpTestGroup = new HttpTestGroup(TestName);
        container.Results.Add(_httpTestGroup);
    }

    public async Task Run()
    {
        await GetLimit();
    }

    private async Task GetLimit()
    {
        LimitBase? limit = null;

        await _httpTestGroup.AddTest(
            testName: $"Get Limit",
            testFunction: async () => limit = await _client.Monitor.Limits.Get(),
            onSuccess: httpTestData => httpTestData.Data = JsonSerializer.SerializeToElement(limit, limit!.GetType(), GlobalJsonOptions.Options)
          );
    }
}
