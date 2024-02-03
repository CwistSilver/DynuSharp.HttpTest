using DynuSharp.Data.Email;
using DynuSharp.Data.Email.Type;
using DynuSharp.HttpTest.Data;
using DynuSharp.HttpTest.Utilities;
using System.Text.Json;

namespace DynuSharp.HttpTest.Client.Email;
public sealed class EmailHttpTest : IHttpTest
{
    private const string TestName = "Email";
    private readonly IDynuClient _client;
    private readonly HttpTestGroup _httpTestGroup;
    private DynuEmailServiceBase? _dynuEmailServiceBase;

    public EmailHttpTest(IDynuClient client, TestContainer container, DynuEmailServiceBase? dynuEmailServiceBase)
    {
        _client = client;
        _dynuEmailServiceBase = dynuEmailServiceBase;
        _httpTestGroup = new HttpTestGroup(TestName);
        container.Results.Add(_httpTestGroup);
    }

    public async Task Run()
    {
        var emails = await GetEmails();
        if (emails is null || emails.Count == 0)
            return;

        if (_dynuEmailServiceBase is null)
            _dynuEmailServiceBase = emails[0];

        await GetEmailUsingGeneric(_dynuEmailServiceBase.Id);
        await GetEmailWithoutGeneric(_dynuEmailServiceBase.Id);
        await GetDeliveryQueue(_dynuEmailServiceBase.Id);
    }

    public async Task<IReadOnlyList<DynuEmailServiceBase>?> GetEmails()
    {
        IReadOnlyList<DynuEmailServiceBase>? emails = null;
        var jsonElements = new List<JsonElement>();

        await _httpTestGroup.AddTest(
          testName: $"Get Email services",
          testFunction: async () =>
          {
              emails = await _client.Email.GetListAsync();
              foreach (var email in emails)
                  jsonElements.Add(JsonSerializer.SerializeToElement(email, email.GetType(), GlobalJsonOptions.Options));
          },
          onSuccess: httpTestData => httpTestData.Data = JsonSerializer.SerializeToElement(jsonElements, GlobalJsonOptions.Options)
        );

        return emails;
    }

    private async Task GetDeliveryQueue(int emailId)
    {
        IReadOnlyList<QueuedEmailMessage>? dynuEmailDeliveryQueueMessages = null;

        await _httpTestGroup.AddTest(
            testName: $"Get Email Delivery Queue Messages for mail service with the ID '{emailId}'",
            testFunction: async () => dynuEmailDeliveryQueueMessages = await _client.Email.GetDeliveryQueueAsync(emailId),
            onSuccess: httpTestData => httpTestData.Data = JsonSerializer.SerializeToElement(dynuEmailDeliveryQueueMessages, GlobalJsonOptions.Options)
        );
    }

    private async Task GetEmailUsingGeneric(int emailId)
    {
        DynuEmailServiceBase? emailDetails = null;

        await _httpTestGroup.AddTest(
            testName: $"Get Email service with the ID '{emailId}' using generic method",
            testFunction: async () => emailDetails = await _client.Email.GetAsync(emailId),
            onSuccess: httpTestData => httpTestData.Data = JsonSerializer.SerializeToElement(emailDetails, emailDetails!.GetType(), GlobalJsonOptions.Options)
        );
    }

    async Task GetEmailWithoutGeneric(int emailId)
    {
        DynuEmailServiceBase? emailDetails = null;

        await _httpTestGroup.AddTest(
            testName: $"Get Email service with the ID '{emailId}' without generic method",
            testFunction: async () => emailDetails = await _client.Email.GetAsync<DynuEmailServiceBase>(emailId),
            onSuccess: httpTestData => httpTestData.Data = JsonSerializer.SerializeToElement(emailDetails, emailDetails!.GetType(), GlobalJsonOptions.Options)
        );
    }
}
