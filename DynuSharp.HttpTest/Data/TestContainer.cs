using DynuSharp.HttpTest.Utilities;
using Spectre.Console;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DynuSharp.HttpTest.Data;
public class TestContainer
{
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [JsonPropertyName("groupResults")]
    public ObservableCollection<HttpTestGroup> Results { get; set; } = new ObservableCollection<HttpTestGroup>();

    private readonly SemaphoreSlim _semaphoreSlim = new(1, 1);

    private Guid currentLiveId = Guid.Empty;


    public async Task RunTests(Configuration configuration, IDynuClient dynuClient)
    {
        Results.CollectionChanged += GroupResults_CollectionChanged;

        foreach (var group in configuration.Groups)
        {
            await group.Run(dynuClient, this);
        }

        await StopLiveTable();

        Results.CollectionChanged -= GroupResults_CollectionChanged;



        await Save(configuration);
    }

    public async Task CreateTestFile(string filename)
    {
        await StopLiveTable();

        var newFileName = GetFileName(filename);

        var jsonData = JsonSerializer.Serialize(this, GlobalJsonOptions.Options);

        await File.WriteAllTextAsync(newFileName, jsonData);
        var fullpath = Path.Combine(Directory.GetCurrentDirectory(), newFileName);
        PrintSavedFile(fullpath);
    }

    private async void GroupResults_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems is null || e.Action != System.Collections.Specialized.NotifyCollectionChangedAction.Add) return;

        await StopLiveTable();
        var httpTestGroup = Results.Last();
        await StartLiveView(httpTestGroup);
    }

    private async Task StartLiveView(HttpTestGroup httpTestGroup)
    {
        currentLiveId = httpTestGroup.Id;
        await AnsiConsole.Live(httpTestGroup.Table).StartAsync(LiveTable);

        var tablePadder = new Padder(new Text("")).PadRight(0).PadBottom(0).PadTop(3);
        AnsiConsole.Write(tablePadder);
        _semaphoreSlim.Release();
    }

    private async Task LiveTable(LiveDisplayContext ctx)
    {
        var startId = Guid.Parse(currentLiveId.ToString());
        while (startId == currentLiveId)
        {
            ctx.Refresh();
            await Task.Delay(100);
        }
    }

    public void SetTaskMessage(string message)
    {
        Console.Title = message;
    }

    private async Task StopLiveTable()
    {
        currentLiveId = Guid.Empty;
        await _semaphoreSlim.WaitAsync();
    }


    private async Task Save(Configuration configuration)
    {
        var newFileName = GetFileName($"{configuration.Name}_results_{DateTime.Now.ToShortDateString().Replace('.', '-')}");
        var fullpath = Path.Combine(Directory.GetCurrentDirectory(), newFileName);

        var jsonData = JsonSerializer.Serialize(this, GlobalJsonOptions.Options);

        await File.WriteAllTextAsync(newFileName, jsonData);

        PrintSavedFile(fullpath);
    }

    private static void PrintSavedFile(string filePath)
    {
        var fileuri = new Uri(filePath).AbsoluteUri;
        var currentDirectory = Directory.GetCurrentDirectory();
        AnsiConsole.MarkupLine($"Created [blue][link={fileuri}]Results File[/][/] in directory [yellow]{currentDirectory}[/].");
    }

    private static string GetFileName(string filename)
    {
        var filepath = $"{filename}.json";
        var index = 0;
        while (File.Exists(filepath))
        {
            index++;
            filepath = $"{filename}({index}).json";
        }
        return filepath;
    }
}
