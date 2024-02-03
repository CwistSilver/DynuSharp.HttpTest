using DynuSharp.Data.Dns;
using DynuSharp.HttpTest.Data;
using DynuSharp.HttpTest.Utilities;
using System.Text.Json;

namespace DynuSharp.HttpTest.Client.Dns;
internal class DnsIpUpdateHistoryHttpTest : IHttpTest
{
    private const string TestName = "DNS Ip Update Test";
    private readonly IDynuClient _client;
    private readonly HttpTestGroup _httpTestGroup;

    internal DnsIpUpdateHistoryHttpTest(IDynuClient client, TestContainer container)
    {
        _client = client;

        _httpTestGroup = new HttpTestGroup(TestName);
        container.Results.Add(_httpTestGroup);
    }

    public async Task Run() => await GetIpUpdateHistory();

    private async Task GetIpUpdateHistory()
    {
        IReadOnlyList<DnsIpUpdate>? dnsIpUpdates = null;

        await _httpTestGroup.AddTest(
            testName: $"Get IP update history",
            testFunction: async () => dnsIpUpdates = await _client.DNS.GetIpUpdateHistoryAsync(),
            onSuccess: httpTestData => httpTestData.Data = JsonSerializer.SerializeToElement(dnsIpUpdates, GlobalJsonOptions.Options)
          );
    }
}
