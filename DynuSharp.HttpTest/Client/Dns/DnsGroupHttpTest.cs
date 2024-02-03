using DynuSharp.Data.Dns;
using DynuSharp.HttpTest.Data;
using DynuSharp.HttpTest.Utilities;
using System.Text.Json;

namespace DynuSharp.HttpTest.Client.Dns;
internal class DnsGroupHttpTest : IHttpTest
{
    private const string TestName = "DNS Group";
    private readonly IDynuClient _client;
    private readonly HttpTestGroup _httpTestGroup;

    internal DnsGroupHttpTest(IDynuClient client, TestContainer container)
    {
        _client = client;

        _httpTestGroup = new HttpTestGroup(TestName);
        container.Results.Add(_httpTestGroup);
    }

    public async Task Run()
    {
        await GetGroups();
        var addedGroup = await AddGroup();
        if (addedGroup is null)
            return;

        await UpdateGroup(addedGroup);
        await DeleteGroup(addedGroup);
    }

    private async Task GetGroups()
    {
        IReadOnlyList<DnsGroup> dnsGroups = null;

        await _httpTestGroup.AddTest(
            testName: $"Get Groups",
            testFunction: async () => dnsGroups = await _client.DNS.Groups.GetListAsync(),
            onSuccess: httpTestData => httpTestData.Data = JsonSerializer.SerializeToElement(dnsGroups, GlobalJsonOptions.Options)
          );
    }

    private async Task DeleteGroup(DnsGroup selectedGroup)
    {
        await _httpTestGroup.AddTest(
            testName: $"Delete Group: {selectedGroup.Id} ({selectedGroup.GroupName})",
            testFunction: async () => await _client.DNS.Groups.DeleteAsync(selectedGroup.Id)
          );
    }

    private async Task<DnsGroup?> AddGroup()
    {
        DnsGroup? addedDnsGroup = null;

        await _httpTestGroup.AddTest(
            testName: $"Add Group",
            testFunction: async () =>
            {
                var dnsGroup = new DnsGroup()
                {
                    GroupName = "NewDynuApiGroup"
                };

                addedDnsGroup = await _client.DNS.Groups.AddAsync(dnsGroup);
            },
            onSuccess: httpTestData => httpTestData.Data = JsonSerializer.SerializeToElement(addedDnsGroup, GlobalJsonOptions.Options)
          );

        return addedDnsGroup;
    }

    private async Task UpdateGroup(DnsGroup selectedGroup)
    {
        DnsGroup? updatedDnsGroup = null;

        await _httpTestGroup.AddTest(
            testName: $"Update Group: {selectedGroup.Id} ({selectedGroup.GroupName})",
            testFunction: async () =>
            {
                var dnsGroup = new DnsGroup()
                {
                    Id = selectedGroup.Id,
                    GroupName = selectedGroup.GroupName,
                    GroupPassword = "1wq32P124"
                };

                updatedDnsGroup = await _client.DNS.Groups.UpdateAsync(selectedGroup.Id, dnsGroup);
            },
            onSuccess: httpTestData => httpTestData.Data = JsonSerializer.SerializeToElement(updatedDnsGroup, GlobalJsonOptions.Options)
          );
    }
}
