using DynuSharp.Data.Email;
using DynuSharp.Data.Email.Type;
using DynuSharp.HttpTest.Data;
using DynuSharp.HttpTest.Utilities;
using System.Text.Json;

namespace DynuSharp.HttpTest.Client.Email;
public sealed class EmailBlacklistHttpTest : IHttpTest
{
    private const string TestName = "Email Blacklist";
    private readonly IDynuClient _client;
    private readonly HttpTestGroup _httpTestGroup;
    private readonly DynuEmailServiceBase _dynuEmailBase;

    public EmailBlacklistHttpTest(IDynuClient client, TestContainer container, DynuEmailServiceBase dynuEmailBase)
    {
        _client = client;
        _dynuEmailBase = dynuEmailBase;
        _httpTestGroup = new HttpTestGroup(TestName);
        container.Results.Add(_httpTestGroup);
    }

    public async Task Run()
    {
        var blacklist = await GetBlacklist();
        if (blacklist is null || blacklist.Count == 0)
            return;

        await GetEmailBlacklistItem(blacklist[0].Id);
    }

    public async Task<IReadOnlyList<DynuEmailBlacklistItem>?> GetBlacklist()
    {
        IReadOnlyList<DynuEmailBlacklistItem>? blacklist = null;

        await _httpTestGroup.AddTest(
          testName: $"Get Blacklist for Email Service: {_dynuEmailBase.Id} ({_dynuEmailBase.Name})",
          testFunction: async () =>
          {
              blacklist = await _client.Email.Blacklist.GetListAsync(_dynuEmailBase.Id);
          },
          onSuccess: httpTestData => httpTestData.Data = JsonSerializer.SerializeToElement(blacklist, GlobalJsonOptions.Options)
        );

        return blacklist;
    }

    async Task GetEmailBlacklistItem(int blacklistId)
    {
        DynuEmailBlacklistItem? blacklistItem = null;

        await _httpTestGroup.AddTest(
            testName: $"Get BlacklistItem with the ID '{blacklistId}' for the Email Service: {_dynuEmailBase.Id} ({_dynuEmailBase.Name})",
            testFunction: async () => blacklistItem = await _client.Email.Blacklist.GetAsync(_dynuEmailBase.Id, blacklistId),
            onSuccess: httpTestData => httpTestData.Data = JsonSerializer.SerializeToElement(blacklistItem, GlobalJsonOptions.Options)
        );
    }
}
