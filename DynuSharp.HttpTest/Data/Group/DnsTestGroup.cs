using DynuSharp.Data.Dns;
using DynuSharp.HttpTest.Client.Dns;
using Spectre.Console;

namespace DynuSharp.HttpTest.Data.Group;
public class DnsTestGroup : ITestGroup
{
    private const string IpUpdateTestName = "DNS IP update history";
    private const string RecordTestName = "DNS Record";
    private const string DnssecTestName = "DNSSEC";
    private const string DomainTestName = "DNS Domain";
    private const string GroupTestName = "DNS Group";
    private const string LimitTestName = "DNS Limit";
    private const string WebRedirectTestName = "DNS WebRedirect";

    private static IReadOnlyList<string> _groupItems = new List<string>
    {
        IpUpdateTestName, RecordTestName,DnssecTestName,DomainTestName,GroupTestName, LimitTestName, WebRedirectTestName
    };

    private static IReadOnlyList<string> DnsDomainRequiredList { get; } = new List<string> { RecordTestName, DnssecTestName, LimitTestName, WebRedirectTestName };

    public string Name { get; } = "Dns";
    public List<string> SelectedItems { get; set; } = new List<string>();
    public IReadOnlyList<string> GroupItems => _groupItems;

    private DnsDomain? _dnsDomain;


    public async Task Run(IDynuClient client, TestContainer container)
    {
        if (SelectedItems.Contains(IpUpdateTestName))
        {
            var ipUpdateHttpTest = new DnsIpUpdateHistoryHttpTest(client, container);
            await ipUpdateHttpTest.Run();
        }

        if (SelectedItems.Contains(GroupTestName))
        {
            var dnsGroupHttpTest = new DnsGroupHttpTest(client, container);
            await dnsGroupHttpTest.Run();
        }

        if (SelectedItems.Contains(DomainTestName))
        {
            var dnsDomains = new DnsDomainHttpTest(client, container);
            var createdDnsDomain = await dnsDomains.Run();

            if (createdDnsDomain is not null)
                _dnsDomain = createdDnsDomain;
            else if (_dnsDomain is null)
                return;
        }

        if (SelectedItems.Contains(RecordTestName))
        {
            var dnsRecordHttpTest = new DnsRecordHttpTest(client, container, _dnsDomain);
            await dnsRecordHttpTest.Run();
        }

        if (SelectedItems.Contains(DnssecTestName))
        {
            var dnssecHttpTest = new DnssecHttpTest(client, container, _dnsDomain);
            await dnssecHttpTest.Run();
        }

        if (SelectedItems.Contains(LimitTestName))
        {
            var limitHttpTest = new DnsLimitHttpTest(client, container, _dnsDomain);
            await limitHttpTest.Run();
        }

        if (SelectedItems.Contains(WebRedirectTestName))
        {
            var webRedirectHttpTest = new WebRedirectHttpTest(client, container, _dnsDomain);
            await webRedirectHttpTest.Run();
        }
    }

    public async Task<bool> Prepare(IDynuClient dynuClient)
    {
        if (RequiredDnsDomain())
        {
            var selectedDnsDomain = await SelectDnsDomain(dynuClient);
            if (!selectedDnsDomain)
                return false;
        }

        return true;
    }

    private async Task<bool> SelectDnsDomain(IDynuClient dynuClient)
    {
        try
        {
            var existingDomains = await dynuClient.DNS.Domains.GetListAsync();
            if (!existingDomains.Any())
                return true;

            var domainNames = new List<string>();
            domainNames = existingDomains.Select(x => x.Name).ToList();
            if (SelectedItems.Contains(DomainTestName))
            {
                domainNames.Insert(0, $"Use domain Created by testing '{DomainTestName}'");
            }

            var domainSelectionPrompt = new SelectionPrompt<string>()
                       .Title("Select a [green]DnsDomains[/] you want do test?")
                       .PageSize(50)
                       .AddChoices(domainNames);


            var dnsDomainName = AnsiConsole.Prompt(domainSelectionPrompt);
            if (dnsDomainName == domainNames[0])
                return true;

            _dnsDomain = existingDomains.FirstOrDefault(x => x.Name == dnsDomainName);

            return true;
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteException(ex);
            return await PromptForDomainId(dynuClient);
        }
    }

    private async Task<bool> PromptForDomainId(IDynuClient dynuClient)
    {
        var selectedDomainId = AnsiConsole.Prompt(new TextPrompt<int>("Please enter a valid [yellow]Dns Domain[/] ID").PromptStyle("yellow"));

        try
        {
            _dnsDomain = await dynuClient.DNS.Domains.GetAsync(selectedDomainId);
            return true;
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteException(ex);
            return false;
        }
    }

    private bool RequiredDnsDomain() => SelectedItems.Any(x => DnsDomainRequiredList.Contains(x));
}
