using DynuSharp.Authentication;
using DynuSharp.HttpTest.Data;
using DynuSharp.HttpTest.Data.Group;
using Spectre.Console;

namespace DynuSharp.HttpTest.Utilities;
internal static class AnsiConsoleHelper
{
    private const string EnterApiKeyPrompt = "Please enter your API key";
    private const string EnterClientIdPrompt = "Please enter your client id";
    private const string EnterClientSecretPrompt = "Please enter your client secret";
    private const string AuthenticationSelectionPromptTitle = "Which authentication method do you want to use?";
    private const string ConfigurationSelectTitle = "{0} configuration(s) where found. Do you want to run a previous configuration again?";
    private const string ConfigurationSelectionPromptTitle = "Select the [green]configuration[/] you want do run again?";

    private const string SecurityNoticeConfirmPrompt = "Are you aware of the risks and agree to proceed?";
    private const string SecurityNoticeTitle = "[bold yellow on red]IMPORTANT SECURITY NOTICE:[/]";
    private const string SecurityNotice = "[invert]This program performs direct interactions with the production system. Although it has been carefully designed to ensure data safety and accuracy, there is always a risk of unintended data modifications or deletions. Especially in cases of connection interruptions or unexpected errors, changes made by the program may not be reversible. This could result in incomplete data or system configurations not being reset to their original state. By using this program, you acknowledge these risks and agree to take full responsibility for any consequences. Please be aware of the potential impacts and proceed with caution.[/]";

    private const string ApiKeyTitle = "API key";
    private const string OAuthTitle = "OAuth";

    private static List<ITestGroup> TestGroups => new List<ITestGroup>
    {
        new GeneralTestGroup(),
        new DnsTestGroup(),
        new EmailTestGroup(),
        new MonitorTestGroup()
    };

    internal static async Task SelectAndRunTestConfig(IDynuClient dynuClient)
    {
        if (await TryRunPreviousConfig(dynuClient))
            return;

        var httpTestRunner = new Configuration() { Name = $"Dynu test configuration" };
        httpTestRunner.Groups.AddRange(TestGroups);

        httpTestRunner.SelectTests();

        if (!await httpTestRunner.PrepareGroups(dynuClient))
            return;

        httpTestRunner.ShowSelection();

        await httpTestRunner.RunTests(dynuClient);
    }



    internal static bool ConfirmSecurityNotice()
    {
        AnsiConsole.MarkupLine(SecurityNoticeTitle);
        AnsiConsole.MarkupLine(SecurityNotice);

        AnsiConsole.WriteLine();
        return AnsiConsole.Confirm(SecurityNoticeConfirmPrompt, false);
    }



    internal static IAuthentication SelectAuthentication()
    {
        var authenticationMethod = AnsiConsole.Prompt(
          new SelectionPrompt<string>()
              .Title(AuthenticationSelectionPromptTitle)
              .AddChoices(ApiKeyTitle, OAuthTitle));

        switch (authenticationMethod)
        {
            case ApiKeyTitle:
                {
                    var apiKey = AnsiConsole.Prompt(new TextPrompt<string>(EnterApiKeyPrompt).PromptStyle("red").Secret());
                    return new ApiKeyAuthentication(apiKey);
                }

            case OAuthTitle:
                {
                    var clientId = AnsiConsole.Prompt(new TextPrompt<string>(EnterClientIdPrompt).PromptStyle("red").Secret());
                    var clientSecret = AnsiConsole.Prompt(new TextPrompt<string>(EnterClientSecretPrompt).PromptStyle("red").Secret());
                    return new OAuth2Authentication(clientId, clientSecret);
                }

            default:
                throw new NotImplementedException();
        }
    }


    private static async Task<bool> TryRunPreviousConfig(IDynuClient dynuClient)
    {
        var configurations = await Configuration.LoadConfigurations(Directory.GetCurrentDirectory());
        if (configurations.Count == 0)
            return false;

        var configurationNames = new List<string>();
        foreach (var item in configurations)
            configurationNames.Add(item.Name);

        var runAgain = AnsiConsole.Confirm(string.Format(ConfigurationSelectTitle, configurations.Count));

        if (!runAgain)
            return false;

        var selectionPrompt = new SelectionPrompt<string>()
            .Title(ConfigurationSelectionPromptTitle)
            .PageSize(50)
            .AddChoices(configurationNames);

        var selectedTests = AnsiConsole.Prompt(selectionPrompt);
        if (selectedTests is null)
            return false;

        var selectedTest = configurations.FirstOrDefault(x => x.Name == selectedTests);
        if (selectedTest is null)
            return false;

        selectedTest.ShowSelection();

        if (!await selectedTest.PrepareGroups(dynuClient))
            return false;

        await selectedTest.RunTests(dynuClient, false);

        return true;
    }
}
