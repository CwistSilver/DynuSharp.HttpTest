using DynuSharp.Data.Email.Type;
using DynuSharp.HttpTest.Client.Email;
using Spectre.Console;

namespace DynuSharp.HttpTest.Data.Group;
public class EmailTestGroup : ITestGroup
{
    private const string BaseTestName = "Email Base";
    private const string BlacklistTestName = "Email Blacklist";
    private const string WhitelistTestName = "Email Whitelist";
    private static IReadOnlyList<string> _groupItems = new List<string>
    {
        BaseTestName, BlacklistTestName, WhitelistTestName
    };

    private static IReadOnlyList<string> EmailServiceRequiredList { get; } = new List<string> { BlacklistTestName, WhitelistTestName };


    public string Name { get; } = "Email";
    public List<string> SelectedItems { get; set; } = new List<string>();
    public IReadOnlyList<string> GroupItems => _groupItems;

    private DynuEmailServiceBase? _dynuEmailServiceBase;

    public async Task Run(IDynuClient client, TestContainer container)
    {
        if (SelectedItems.Contains(BaseTestName))
        {
            var emailHttpTest = new EmailHttpTest(client, container, _dynuEmailServiceBase);
            await emailHttpTest.Run();
        }

        if (SelectedItems.Contains(BlacklistTestName))
        {
            EmailBlacklistHttpTest emailBlacklistHttpTest = new EmailBlacklistHttpTest(client, container, _dynuEmailServiceBase!);
            await emailBlacklistHttpTest.Run();
        }

        if (SelectedItems.Contains(WhitelistTestName))
        {
            EmailWhitelistHttpTest emailWhitelistHttpTest = new EmailWhitelistHttpTest(client, container, _dynuEmailServiceBase!);
            await emailWhitelistHttpTest.Run();
        }
    }

    public async Task<bool> Prepare(IDynuClient dynuClient)
    {
        if (RequiredEmailService())
        {
            var selectedEmailService = await SelectEmailService(dynuClient);
            if (!selectedEmailService)
                return false;
        }

        return true;
    }

    private async Task<bool> SelectEmailService(IDynuClient dynuClient)
    {
        try
        {
            var EmailServices = await dynuClient.Email.GetListAsync();
            if (!EmailServices.Any())
            {
                AnsiConsole.MarkupLine("[red]No Email Service found[/]");
                AnsiConsole.MarkupLine("[red]All tests that requierd a Email Service will be skipped[/]");
                RemoveRequiredEmailService();
                return true;
            }


            var domainSelectionPrompt = new SelectionPrompt<DynuEmailServiceBase>()
                       .Title("Select a [green]Email Service[/] you want do test?")
                       .PageSize(50)
                       .AddChoices(EmailServices);

            _dynuEmailServiceBase = AnsiConsole.Prompt(domainSelectionPrompt);
            return true;
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteException(ex);
            return await PromptForEmailServiceId(dynuClient);
        }
    }

    private async Task<bool> PromptForEmailServiceId(IDynuClient dynuClient)
    {
        var selectedEmailServiceId = AnsiConsole.Prompt(new TextPrompt<int>("Please enter a valid [yellow]Email Service[/] ID").PromptStyle("yellow"));

        try
        {
            _dynuEmailServiceBase = await dynuClient.Email.GetAsync(selectedEmailServiceId);
            return true;
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteException(ex);
            return false;
        }
    }

    private void RemoveRequiredEmailService() => SelectedItems.RemoveAll(x => EmailServiceRequiredList.Contains(x));
    private bool RequiredEmailService() => SelectedItems.Any(x => EmailServiceRequiredList.Contains(x));
}
