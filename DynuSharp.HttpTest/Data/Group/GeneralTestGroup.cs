using DynuSharp.HttpTest.Client;
using DynuSharp.HttpTest.Client.Domain;

namespace DynuSharp.HttpTest.Data.Group;
public class GeneralTestGroup : ITestGroup
{
    private const string DomainTestName = "Domain";
    private const string PingTestName = "Ping";

    private static IReadOnlyList<string> _groupItems = new List<string>
    {
        DomainTestName, PingTestName
    };

    public string Name { get; } = GroupName;
    public static string GroupName => "General";
    public List<string> SelectedItems { get; set; } = new List<string>();
    public IReadOnlyList<string> GroupItems => _groupItems;

    public Task<bool> Prepare(IDynuClient dynuClient) => Task.FromResult(true);

    public async Task Run(IDynuClient dynuClient, TestContainer container)
    {
        if (SelectedItems.Contains(PingTestName))
        {
            var pingHttpTest = new PingHttpTest(dynuClient, container);
            await pingHttpTest.Run();
        }

        if (SelectedItems.Contains(DomainTestName))
        {
            var domainHttpTest = new DomainHttpTest(dynuClient, container);
            await domainHttpTest.Run();
        }
    }
}
