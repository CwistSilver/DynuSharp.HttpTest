using DynuDNS.API;
using DynuSharp.HttpTest.Utilities;
using Spectre.Console;

AnsiConsole.Write(new FigletText("DynuSharp Tests").LeftJustified().Color(Color.DarkOliveGreen1));
AnsiConsole.WriteLine();

if (!AnsiConsoleHelper.ConfirmSecurityNotice())
    return;

var authentication = AnsiConsoleHelper.SelectAuthentication();
var client = new DynuClient(authentication);

await AnsiConsoleHelper.SelectAndRunTestConfig(client);
