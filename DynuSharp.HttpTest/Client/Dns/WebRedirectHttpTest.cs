using DynuSharp.Data.Dns;
using DynuSharp.Data.DnsWebRedirect;
using DynuSharp.HttpTest.Data;
using DynuSharp.HttpTest.Utilities;
using System.Text.Json;

namespace DynuSharp.HttpTest.Client.Dns;
internal class WebRedirectHttpTest : IHttpTest
{
    private const string TestName = "DNS Web Redirect";
    private readonly IDynuClient _client;
    private readonly HttpTestGroup _httpTestGroup;
    private readonly DnsDomain _dnsDomain;
    internal WebRedirectHttpTest(IDynuClient client, TestContainer container, DnsDomain dnsDomain)
    {
        _client = client;

        _httpTestGroup = new HttpTestGroup(TestName);
        container.Results.Add(_httpTestGroup);
        _dnsDomain = dnsDomain;
    }

    public async Task Run()
    {
        var redirects = await GetWebRedirectList();
        if (redirects is null) return;

        foreach (var redirect in redirects)
            await GetWebRedirectDetails(redirect.Id);
    }

    private async Task<IReadOnlyList<DnsWebRedirectBase>?> GetWebRedirectList()
    {
        IReadOnlyList<DnsWebRedirectBase>? redirects = null;
        var jsonElements = new List<JsonElement>();

        await _httpTestGroup.AddTest(
            testName: $"Get Web Redirect list",
            testFunction: async () =>
            {
                redirects = await _client.DNS.WebRedirects.GetListAsync(_dnsDomain.Id);
                foreach (var redirect in redirects)
                    jsonElements.Add(JsonSerializer.SerializeToElement(redirect, redirect.GetType(), GlobalJsonOptions.Options));
            },
            onSuccess: httpTestData => httpTestData.Data = JsonSerializer.SerializeToElement(jsonElements, GlobalJsonOptions.Options)
          );

        return redirects;
    }

    private async Task GetWebRedirectDetails(int webRedirectId)
    {
        DnsWebRedirectBase? redirectDetails = null;

        await _httpTestGroup.AddTest(
            testName: $"Get Web Redirect list",
            testFunction: async () => redirectDetails = await _client.DNS.WebRedirects.GetAsync(_dnsDomain.Id, webRedirectId),
            onSuccess: httpTestData => httpTestData.Data = JsonSerializer.SerializeToElement(redirectDetails, GlobalJsonOptions.Options)
          );
    }


}
