using DynuSharp.Data.Dns;
using DynuSharp.Data.Limit;
using DynuSharp.HttpTest.Data;
using DynuSharp.HttpTest.Utilities;
using System.Text.Json;

namespace DynuSharp.HttpTest.Client.Dns;
internal class DnsLimitHttpTest : IHttpTest
{
    private const string TestName = "DNS Limit";
    private readonly IDynuClient _client;
    private readonly HttpTestGroup _httpTestGroup;
    private readonly DnsDomain _dnsDomain;
    internal DnsLimitHttpTest(IDynuClient client, TestContainer container, DnsDomain dnsDomain)
    {
        _client = client;

        _httpTestGroup = new HttpTestGroup(TestName);
        container.Results.Add(_httpTestGroup);
        _dnsDomain = dnsDomain;
    }

    public async Task Run()
    {
        await GetHostnameLimits();
        await GetDnsRecordLimits();
    }

    private async Task GetDnsRecordLimits()
    {
        IReadOnlyList<DnsRecordLimitData>? limits = null;

        await _httpTestGroup.AddTest(
            testName: $"Get Limit",
            testFunction: async () => limits = await _client.DNS.Limits.GetList(_dnsDomain.Id),
            onSuccess: httpTestData => httpTestData.Data = JsonSerializer.SerializeToElement(limits, GlobalJsonOptions.Options)
          );
    }

    private async Task GetHostnameLimits()
    {
        IReadOnlyList<DnsHostnameLimitData>? limits = null;

        await _httpTestGroup.AddTest(
            testName: $"Get Limits",
            testFunction: async () => limits = await _client.DNS.Limits.GetList(),
            onSuccess: httpTestData => httpTestData.Data = JsonSerializer.SerializeToElement(limits, GlobalJsonOptions.Options)
          );
    }
}
