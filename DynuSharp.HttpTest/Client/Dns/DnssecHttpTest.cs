using DynuSharp.Data.Dns;
using DynuSharp.HttpTest.Data;
using DynuSharp.HttpTest.Utilities;
using System.Text.Json;

namespace DynuSharp.HttpTest.Client.Dns;
internal class DnssecHttpTest : IHttpTest
{
    private const string TestName = "DNSSEC";
    private readonly IDynuClient _client;
    private readonly HttpTestGroup _httpTestGroup;
    private readonly DnsDomain _detailedDomainData;
    internal DnssecHttpTest(IDynuClient client, TestContainer container, DnsDomain detailedDomainData)
    {
        _client = client;
        _detailedDomainData = detailedDomainData;

        _httpTestGroup = new HttpTestGroup(TestName);
        container.Results.Add(_httpTestGroup);
    }

    public async Task Run()
    {
        var dnssecData = await GetDnssecDetails();
        if (dnssecData is null) return;

        if (_detailedDomainData.DNSSEC)
        {
            var success = await DisableDnssec();
            if (success)
                await EnableDnssec();
        }
        else
        {
            var success = await EnableDnssec();
            if (success)
                await DisableDnssec();
        }
    }

    private async Task<bool> EnableDnssec()
    {
        bool success = false;

        await _httpTestGroup.AddTest(
            testName: $"Enable DNSSEC for: {_detailedDomainData.Id} ({_detailedDomainData.Name})",
            testFunction: async () => await _client.DNS.DNSSEC.Enable(_detailedDomainData.Id),
            onSuccess: httpTestData => success = true
          );

        return success;
    }

    private async Task<bool> DisableDnssec()
    {
        bool success = false;

        await _httpTestGroup.AddTest(
            testName: $"Disable DNSSEC for: {_detailedDomainData.Id} ({_detailedDomainData.Name})",
            testFunction: async () => await _client.DNS.DNSSEC.Disable(_detailedDomainData.Id),
            onSuccess: httpTestData => success = true
          );

        return success;
    }

    private async Task<DnssecData?> GetDnssecDetails()
    {
        DnssecData? dnssecData = null;

        await _httpTestGroup.AddTest(
            testName: $"Get DNSSEC details",
            testFunction: async () => dnssecData = await _client.DNS.DNSSEC.Get(_detailedDomainData.Id),
            onSuccess: httpTestData => httpTestData.Data = JsonSerializer.SerializeToElement(dnssecData, GlobalJsonOptions.Options)
          );

        return dnssecData;
    }
}
