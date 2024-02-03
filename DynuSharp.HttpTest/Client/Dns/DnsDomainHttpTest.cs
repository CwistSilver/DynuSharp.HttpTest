using DynuSharp.Data;
using DynuSharp.Data.Dns;
using DynuSharp.HttpTest.Data;
using DynuSharp.HttpTest.Utilities;
using System.Text.Json;

namespace DynuSharp.HttpTest.Client.Dns;
internal class DnsDomainHttpTest : IHttpTest<DnsDomain?>
{
    private const string TestName = "DNS Domain";
    private const string TestDomainName = "Dynu-Test-";
    private const string TestDomainString = "Dynu-Test-{0}";
    private readonly IDynuClient _client;
    private readonly HttpTestGroup _httpTestGroup;

    internal DnsDomainHttpTest(IDynuClient client, TestContainer container)
    {
        _client = client;
        _httpTestGroup = new HttpTestGroup(TestName);
        container.Results.Add(_httpTestGroup);
    }

    public async Task<DnsDomain?> Run()
    {
        var domains = await CheckDomainList();
        await DeleteTestDomains(domains);

        var creadtedDomain = await CreateDomain();
        if (creadtedDomain is null)
        {
            return null;
        }

        await UpdateDomain(creadtedDomain);
        await GetAsync(creadtedDomain);

        return creadtedDomain;
    }

    private async Task DeleteTestDomains(IReadOnlyList<DnsDomain>? domains)
    {
        if (domains is not null)
        {
            var oldTestDomains = domains.Where(x => x.Name.ToLower().Contains(TestDomainName.ToLower()));
            foreach (var domain in oldTestDomains)
            {
                await _client.DNS.Domains.DeleteAsync(domain.Id);
            }
        }
    }

    public async Task<DnsDomain?> CreateDomain()
    {
        DnsDomain? addedDnsDomain = null;

        await _httpTestGroup.AddTest(
            testName: "AddAsync",
            testFunction: async () =>
            {
                var domainName = string.Format(TestDomainString, Guid.NewGuid());
                var topDomain = TopLevelDomains.GeneralUseDomains[Random.Shared.Next(TopLevelDomains.GeneralUseDomains.Count)];
                var fullName = $"{domainName}.{topDomain}";
                var domainData = new DnsDomain() { Name = fullName };
                await _client.DNS.Domains.AddAsync(domainData);
                var domains = await _client.DNS.Domains.GetListAsync();
                addedDnsDomain = domains.FirstOrDefault(domainData => domainData.Name.ToLower() == fullName.ToLower());
            },
        onSuccess: httpTestData => httpTestData.Data = JsonSerializer.SerializeToElement(addedDnsDomain, GlobalJsonOptions.Options)
        );

        return addedDnsDomain;
    }


    private async Task UpdateDomain(DnsDomain currentDomain)
    {
        await _httpTestGroup.AddTest(
            testName: "UpdateAsync",
            testFunction: async () =>
            {
                currentDomain.TTL = 120;
                await _client.DNS.Domains.UpdateAsync(currentDomain.Id, currentDomain);
            },
            httpTestData => httpTestData.Data = JsonSerializer.SerializeToElement(currentDomain, GlobalJsonOptions.Options)
        );

    }

    private async Task<IReadOnlyList<DnsDomain>?> CheckDomainList()
    {
        IReadOnlyList<DnsDomain> domains = null;

        await _httpTestGroup.AddTest(
            testName: "GetListAsync",
            testFunction: async () => domains = await _client.DNS.Domains.GetListAsync(),
            onSuccess: httpTestData => httpTestData.Data = JsonSerializer.SerializeToElement(domains, GlobalJsonOptions.Options)
        );

        return domains;
    }

    private async Task GetAsync(DnsDomain domain)
    {
        DnsDomain? getDomainResult = null;
        await _httpTestGroup.AddTest(
            testName: "GetAsync",
            testFunction: async () => getDomainResult = await _client.DNS.Domains.GetAsync(domain.Id),
            onSuccess: httpTestData => httpTestData.Data = JsonSerializer.SerializeToElement(getDomainResult, GlobalJsonOptions.Options)
       );
    }
}
