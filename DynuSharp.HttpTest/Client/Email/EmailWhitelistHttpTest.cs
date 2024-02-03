using DynuSharp.Data.Email;
using DynuSharp.Data.Email.Type;
using DynuSharp.HttpTest.Data;
using DynuSharp.HttpTest.Utilities;
using System.Text.Json;

namespace DynuSharp.HttpTest.Client.Email;
public sealed class EmailWhitelistHttpTest : IHttpTest
{
    private const string TestName = "Email Whitelist";
    private readonly IDynuClient _client;
    private readonly HttpTestGroup _httpTestGroup;
    private readonly DynuEmailServiceBase _dynuEmailBase;

    public EmailWhitelistHttpTest(IDynuClient client, TestContainer container, DynuEmailServiceBase dynuEmailBase)
    {
        _client = client;
        _dynuEmailBase = dynuEmailBase;
        _httpTestGroup = new HttpTestGroup(TestName);
        container.Results.Add(_httpTestGroup);
    }

    public async Task Run()
    {
        var whitelist = await GetWhitelist();
        if (whitelist is null || whitelist.Count == 0)
            return;

        await GetEmailWhitelistItem(whitelist[0].Id);
    }

    public async Task<IReadOnlyList<DynuEmailWhitelistItem>?> GetWhitelist()
    {
        IReadOnlyList<DynuEmailWhitelistItem>? whitelist = null;

        await _httpTestGroup.AddTest(
          testName: $"Get Whitelist for Email Service: {_dynuEmailBase.Id} ({_dynuEmailBase.Name})",
          testFunction: async () =>
          {
              whitelist = await _client.Email.Whitelist.GetListAsync(_dynuEmailBase.Id);
          },
          onSuccess: httpTestData => httpTestData.Data = JsonSerializer.SerializeToElement(whitelist, GlobalJsonOptions.Options)
        );

        return whitelist;
    }

    async Task GetEmailWhitelistItem(int whitelistId)
    {
        DynuEmailWhitelistItem? whitelistItem = null;

        await _httpTestGroup.AddTest(
            testName: $"Get Whitelist with the ID '{whitelistId}' for the Email Service: {_dynuEmailBase.Id} ({_dynuEmailBase.Name})",
            testFunction: async () => whitelistItem = await _client.Email.Whitelist.GetAsync(_dynuEmailBase.Id, whitelistId),
            onSuccess: httpTestData => httpTestData.Data = JsonSerializer.SerializeToElement(whitelistItem, GlobalJsonOptions.Options)
        );
    }
}
