using DynuSharp.Data.Domain;
using DynuSharp.HttpTest.Data;
using DynuSharp.HttpTest.Utilities;
using System.Text.Json;

namespace DynuSharp.HttpTest.Client.Domain;
internal class DomainHttpTest : IHttpTest
{
    private const string TestName = "Domain";
    private readonly IDynuClient _client;
    private readonly HttpTestGroup _httpTestGroup;

    internal DomainHttpTest(IDynuClient client, TestContainer container)
    {
        _client = client;
        _httpTestGroup = new HttpTestGroup(TestName);
        container.Results.Add(_httpTestGroup);
    }

    public async Task Run()
    {
        var domains = await GetAllDomains();
        if (domains is null || domains.Count == 0)
            return;

        var testDomain = domains[0];
        await GetDomainDetails(testDomain);
        if (testDomain.AutoRenewal)
        {
            await DisableAutorenewal(testDomain);
            await EnableAutorenewal(testDomain);
        }
        else
        {
            await EnableAutorenewal(testDomain);
            await DisableAutorenewal(testDomain);
        }

        await LockDomain(testDomain);
        await UnlockDomain(testDomain);

        await GetAllDomainNameServers(testDomain);
        var nameServer = await AddNameServer(testDomain);
        if (nameServer is not null)
        {
            await SetNameServerPrimary(testDomain, nameServer);
            await DeleteNameServer(testDomain, nameServer);
        }
    }

    private async Task<IReadOnlyList<DomainData>?> GetAllDomains()
    {
        IReadOnlyList<DomainData>? domains = null;

        await _httpTestGroup.AddTest(
            testName: "Get domain list",
            testFunction: async () => domains = await _client.Domain.GetListAsync(),
            onSuccess: httpTestData => httpTestData.Data = JsonSerializer.SerializeToElement(domains, GlobalJsonOptions.Options)
         );

        return domains;
    }

    private async Task<IReadOnlyList<NameServer>?> GetAllDomainNameServers(DomainData domainData)
    {
        IReadOnlyList<NameServer>? nameServers = null;

        await _httpTestGroup.AddTest(
            testName: $"Get name servers for domain: {domainData.Id} ({domainData.Name})",
            testFunction: async () => nameServers = await _client.Domain.GetNameServerListAsync(domainData.Id),
            onSuccess: httpTestData => httpTestData.Data = JsonSerializer.SerializeToElement(nameServers, GlobalJsonOptions.Options)
         );

        return nameServers;
    }

    private async Task<NameServer?> AddNameServer(DomainData domainData)
    {
        NameServer? nameServer = null;

        await _httpTestGroup.AddTest(
            testName: $"Add name server for domain: {domainData.Id} ({domainData.Name})",
            testFunction: async () =>
            {
                var nameServerToAdd = new NameServer() { Name = $"TEST.COM" };
                await _client.Domain.AddNameServerAsync(domainData.Id, nameServerToAdd);
                nameServer = nameServerToAdd;
            },
            onSuccess: httpTestData => httpTestData.Data = JsonSerializer.SerializeToElement(nameServer, GlobalJsonOptions.Options)
         );

        return nameServer;
    }

    private async Task<bool> SetNameServerPrimary(DomainData domainData, NameServer nameServer)
    {
        bool success = false;
        await _httpTestGroup.AddTest(
            testName: $"Set name server '{nameServer.Name}' primary for domain: {domainData.Id} ({domainData.Name})",
            testFunction: async () => await _client.Domain.SetNameServerPrimaryAsync(domainData.Id, nameServer),
            onSuccess: httpTestData => success = true
         );

        return success;
    }

    private async Task<bool> DeleteNameServer(DomainData domainData, NameServer nameServer)
    {
        bool success = false;
        await _httpTestGroup.AddTest(
            testName: $"Delete name server '{nameServer.Name}' for domain: {domainData.Id} ({domainData.Name})",
            testFunction: async () => await _client.Domain.DeleteNameServerAsync(domainData.Id, nameServer),
            onSuccess: httpTestData => success = true
         );

        return success;
    }

    private async Task GetDomainDetails(DomainData domainData)
    {
        DomainData? domain = null;

        await _httpTestGroup.AddTest(
            testName: "Get domain details",
            testFunction: async () => domain = await _client.Domain.GetAsync(domainData.Id),
            onSuccess: httpTestData => httpTestData.Data = JsonSerializer.SerializeToElement(domain, GlobalJsonOptions.Options)
         );
    }

    private async Task<bool> EnableAutorenewal(DomainData domainData)
    {
        bool success = false;
        await _httpTestGroup.AddTest(
            testName: $"Enable autorenewal for domain: {domainData.Id} ({domainData.Name})",
            testFunction: async () => await _client.Domain.EnableAutorenewAsync(domainData.Id),
            onSuccess: httpTestData => success = true
         );

        return success;
    }

    private async Task<bool> DisableAutorenewal(DomainData domainData)
    {
        bool success = false;
        await _httpTestGroup.AddTest(
            testName: $"Disable autorenewal for domain: {domainData.Id} ({domainData.Name})",
            testFunction: async () => await _client.Domain.DisableAutorenewAsync(domainData.Id),
             onSuccess: httpTestData => success = true
         );

        return success;
    }

    private async Task<bool> LockDomain(DomainData domainData)
    {
        bool success = false;
        await _httpTestGroup.AddTest(
            testName: $"Lock domain: {domainData.Id} ({domainData.Name})",
            testFunction: async () => await _client.Domain.LockAsync(domainData.Id),
             onSuccess: httpTestData => success = true
         );

        return success;
    }

    private async Task<bool> UnlockDomain(DomainData domainData)
    {
        bool success = false;
        await _httpTestGroup.AddTest(
            testName: $"Unlock domain: {domainData.Id} ({domainData.Name})",
            testFunction: async () => await _client.Domain.UnlockAsync(domainData.Id),
             onSuccess: httpTestData => success = true
         );

        return success;
    }

}
