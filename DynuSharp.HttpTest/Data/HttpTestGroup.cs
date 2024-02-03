using DynuSharp.HttpTest.Utilities;
using Spectre.Console;
using Spectre.Console.Json;
using Spectre.Console.Rendering;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DynuSharp.HttpTest.Data;
public class HttpTestGroup
{
    [JsonIgnore]
    public Guid Id { get; init; } = Guid.NewGuid();
    [JsonIgnore]
    public Table Table { get; private set; } = new Table();

    public string Name { get; }
    public List<HttpTestData> Results { get; set; } = new List<HttpTestData>();

    public HttpTestGroup(string name)
    {
        Name = name;
        InitializeTable();
    }

    private void InitializeTable()
    {
        Table.AddColumn("Test Name");
        Table.AddColumn("Result");

        Table.ShowRowSeparators = true;
        Table.Expand = true;
        Table.Title = new TableTitle(Name);
        Table.Title.Style = new Style(Console.BackgroundColor, Console.ForegroundColor, Decoration.Underline);
        Table.Columns[0].Alignment = Justify.Center;

        Table.Columns[0].Width = (int)(Console.BufferWidth * 0.2f);
        Table.Columns[1].Width = (int)(Console.BufferWidth * 0.75f);

        Table.Border(TableBorder.Rounded);
    }

    private void UpdateTable(HttpTestData httpTestData)
    {
        var testName = new Text(httpTestData.Name);
        var header = httpTestData.Result == HttpTestResult.Successful ? "Successful" : "Failed";
        var color = httpTestData.Result == HttpTestResult.Successful ? Color.Green : Color.Red;
        Renderable content = new Markup(httpTestData.Result == HttpTestResult.Successful ? "[green]Successful[/]" : "[red]Failed[/]");

        if (httpTestData.Data is not null)
        {
            var contentString = JsonSerializer.Serialize(httpTestData.Data, GlobalJsonOptions.Options);
            if (contentString is not null)
            {
                var jsonText = new JsonText(contentString);

                content = new Panel(jsonText)
                    .Header(header)
                    .Expand()
                    .RoundedBorder()
                    .BorderColor(color);
            }

        }

        Table.AddRow(testName, content);
    }

    public async Task AddTest(string testName, Func<Task> testFunction, Action<HttpTestData> onSuccess = null)
    {
        var httpTestData = new HttpTestData() { Name = testName };
        var isSuccessful = await PerformCheckAction(testFunction, httpTestData);
        if (isSuccessful)
            onSuccess?.Invoke(httpTestData);

        UpdateTable(httpTestData);
    }

    private async Task<bool> PerformCheckAction(Func<Task> checkAction, HttpTestData httpTestData)
    {
        try
        {
            await checkAction();
            httpTestData.Result = HttpTestResult.Successful;
            Results.Add(httpTestData);

            return true;
        }
        catch (Exception ex)
        {
            httpTestData.Result = HttpTestResult.Failed;
            httpTestData.Data = ex.ToJsonObject();
            Results.Add(httpTestData);

            return false;
        }
    }
}