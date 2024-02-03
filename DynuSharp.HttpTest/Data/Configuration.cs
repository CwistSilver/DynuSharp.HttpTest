using DynuSharp.HttpTest.Data.Group;
using DynuSharp.HttpTest.Utilities;
using Spectre.Console;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DynuSharp.HttpTest.Data;
public class Configuration
{

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("groups")]
    public List<ITestGroup> Groups { get; set; } = new List<ITestGroup>();

    public Configuration()
    {
        Name = string.Empty;
        CreatedAt = DateTime.Now;
    }

    public void SelectTests()
    {
        var selectionPrompt = new MultiSelectionPrompt<string>()
                    .Title("Select the [green]Modules[/] you want do test?")
                    .Required().PageSize(50)
                    .InstructionsText(
                        "[grey](Press [blue]<space>[/] to toggle a test, " +
                        "[green]<enter>[/] to accept)[/]");

        foreach (var group in Groups)
        {
            if (GeneralTestGroup.GroupName == group.Name)
                selectionPrompt.AddChoices(group.GroupItems);
            else
                selectionPrompt.AddChoiceGroup(group.Name, group.GroupItems);
        }

        var selectedItems = AnsiConsole.Prompt(selectionPrompt);
        SetSelectetItems(selectedItems);
    }

    public void ShowSelection()
    {
        var root = new Tree("Selected Items");
        foreach (var group in Groups)
        {
            if (group.SelectedItems.Count == 0)
                continue;

            var groupNode = root.AddNode($"[Yellow]{group.Name}[/]");
            foreach (var item in group.SelectedItems)
            {
                groupNode.AddNode($"[blue]{item}[/]");
            }
        }

        AnsiConsole.Write(root);
    }



    public async Task<bool> PrepareGroups(IDynuClient dynuClient)
    {
        foreach (var group in Groups)
        {
            var result = await group.Prepare(dynuClient);
            if (!result)
                return false;
        }

        return true;
    }

    public async Task RunTests(IDynuClient dynuClient, bool askForSave = true)
    {
        var testContainer = new TestContainer();
        await testContainer.RunTests(this, dynuClient);

        if (askForSave)
            await AskForSave();
    }

    public async Task AskForSave()
    {
        if (!AnsiConsole.Confirm("Do you want to save the configuration?"))
            return;

        Name = AnsiConsole.Ask("Please enter a name", Name);
        var newFileName = GetFileName(Name);

        var jsonData = JsonSerializer.Serialize(this, GlobalJsonOptions.Options);

        await File.WriteAllTextAsync(newFileName, jsonData);
        var fullpath = Path.Combine(Directory.GetCurrentDirectory(), newFileName);
        PrintTestFileMessage(fullpath);
    }



    public static async Task<List<Configuration>> LoadConfigurations(string directory)
    {
        var testFiles = Directory.GetFiles(directory, "*.json");
        var testConfigurations = new List<Configuration>();

        foreach (var file in testFiles)
        {
            try
            {
                var jsonData = await File.ReadAllTextAsync(file);
                var testConfiguration = JsonSerializer.Deserialize<Configuration>(jsonData, GlobalJsonOptions.Options);

                if (testConfiguration is null || string.IsNullOrEmpty(testConfiguration.Name))
                    continue;

                testConfigurations.Add(testConfiguration);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        return testConfigurations;
    }

    private void SetSelectetItems(List<string> selectedItems)
    {
        foreach (var group in Groups)
        {
            foreach (var item in group.GroupItems)
            {
                if (selectedItems.Contains(item))
                    group.SelectedItems.Add(item);
            }
        }
    }

    private static void PrintTestFileMessage(string filePath)
    {
        var fileuri = new Uri(filePath).AbsoluteUri;
        var currentDirectory = Directory.GetCurrentDirectory();
        AnsiConsole.MarkupLine($"Created [blue][link={fileuri}]Configuration File[/][/] in directory [yellow]{currentDirectory}[/].");
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
