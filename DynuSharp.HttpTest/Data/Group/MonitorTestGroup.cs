using DynuSharp.HttpTest.Client.Monitor;

namespace DynuSharp.HttpTest.Data.Group;
public class MonitorTestGroup : ITestGroup
{
    private const string BaseTestName = "Monitor Base";
    private const string LimitTestName = "Monitor Limit";
    private static IReadOnlyList<string> _groupItems = new List<string>
    {
        BaseTestName, LimitTestName
    };

    public string Name { get; } = "Monitor";
    public List<string> SelectedItems { get; set; } = new List<string>();
    public IReadOnlyList<string> GroupItems => _groupItems;

    public Task<bool> Prepare(IDynuClient dynuClient) => Task.FromResult(true);

    public async Task Run(IDynuClient dynuClient, TestContainer container)
    {
        if (SelectedItems.Contains(BaseTestName))
        {
            var monitorHttpTest = new MonitorHttpTest(dynuClient, container);
            await monitorHttpTest.Run();
        }

        if (SelectedItems.Contains(LimitTestName))
        {
            var limitHttpTest = new LimitHttpTest(dynuClient, container);
            await limitHttpTest.Run();
        }
    }
}
