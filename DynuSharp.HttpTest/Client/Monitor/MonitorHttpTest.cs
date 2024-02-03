using DynuSharp.Data.Monitor;
using DynuSharp.HttpTest.Data;
using DynuSharp.HttpTest.Utilities;
using System.Text.Json;

namespace DynuSharp.HttpTest.Client.Monitor;
public class MonitorHttpTest : IHttpTest
{
    private const string TestName = "Monitor";
    private readonly IDynuClient _client;
    private readonly HttpTestGroup _httpTestGroup;

    internal MonitorHttpTest(IDynuClient client, TestContainer container)
    {
        _client = client;
        _httpTestGroup = new HttpTestGroup(TestName);
        container.Results.Add(_httpTestGroup);
    }

    public async Task Run()
    {
        var monitors = await GetMonitors();
        var addedMonitor = await AddMonitor();

        var selectedMonitor = addedMonitor;
        if (selectedMonitor is null && monitors is not null && monitors.Count != 0)
            selectedMonitor = monitors[0];

        if (selectedMonitor is not null)
        {
            if (selectedMonitor.State == MonitorState.PAUSED)
            {
                var success = await UnpauseMonitor(selectedMonitor);
                if (success)
                    await PauseMonitor(selectedMonitor);
            }
            else
            {
                var success = await PauseMonitor(selectedMonitor);
                if (success)
                    await UnpauseMonitor(selectedMonitor);
            }
        }

        if (addedMonitor is null) return;
        await DeleteMonitor(addedMonitor);
    }

    public async Task<MonitorBase?> AddMonitor()
    {
        MonitorBase? addedMonitor = null;

        await _httpTestGroup.AddTest(
            testName: "AddAsync",
            testFunction: async () =>
            {
                var monitorName = "HTTP monitor for www.dynu.com";
                var monitorData = new MonitorHTTP() { Name = monitorName, CheckInterval = 10, Url = "https://www.dynu.com", AuthenticationType = "NONE" };
                await _client.Monitor.AddAsync(monitorData);
                var monitors = await _client.Monitor.GetListAsync();
                addedMonitor = monitors.FirstOrDefault(monitor => monitor.Name.ToLower() == monitorName.ToLower());
            },
            onSuccess: httpTestData => httpTestData.Data = JsonSerializer.SerializeToElement(addedMonitor, GlobalJsonOptions.Options)
        );

        return addedMonitor;
    }

    public async Task<IReadOnlyList<MonitorBase>?> GetMonitors()
    {
        IReadOnlyList<MonitorBase>? monitors = null;
        var jsonElements = new List<JsonElement>();

        await _httpTestGroup.AddTest(
          testName: $"Get Monitors",
          testFunction: async () =>
          {
              monitors = await _client.Monitor.GetListAsync();
              foreach (var monitor in monitors)
                  jsonElements.Add(JsonSerializer.SerializeToElement(monitor, monitor.GetType(), GlobalJsonOptions.Options));
          },
          onSuccess: httpTestData => httpTestData.Data = JsonSerializer.SerializeToElement(jsonElements, GlobalJsonOptions.Options)
        );

        return monitors;
    }

    public async Task GetMonitor(MonitorBase monitorBase)
    {
        MonitorBase? monitor = null;

        await _httpTestGroup.AddTest(
          testName: $"Get Monitor {monitorBase.Id} ({monitorBase.Name})",
          testFunction: async () => monitor = await _client.Monitor.GetAsync(monitorBase.Id),
          onSuccess: httpTestData => httpTestData.Data = JsonSerializer.SerializeToElement(monitor, monitor!.GetType(), GlobalJsonOptions.Options)
        );
    }

    public async Task DeleteMonitor(MonitorBase monitorBase)
    {
        await _httpTestGroup.AddTest(
          testName: $"Delete Monitor {monitorBase.Id} ({monitorBase.Name})",
          testFunction: async () => await _client.Monitor.DeleteAsync(monitorBase.Id)
        );
    }

    public async Task<bool> PauseMonitor(MonitorBase monitorBase)
    {
        bool success = false;

        await _httpTestGroup.AddTest(
          testName: $"Pause Monitor {monitorBase.Id} ({monitorBase.Name})",
          testFunction: async () => await _client.Monitor.PauseAsync(monitorBase.Id),
          onSuccess: httpTestData => success = true
        );

        return success;
    }

    public async Task<bool> UnpauseMonitor(MonitorBase monitorBase)
    {
        bool success = false;

        await _httpTestGroup.AddTest(
          testName: $"Unpause Monitor {monitorBase.Id} ({monitorBase.Name})",
          testFunction: async () => await _client.Monitor.UnpauseAsync(monitorBase.Id),
          onSuccess: httpTestData => success = true
        );

        return success;
    }
}
