using System.CommandLine;
using System.CommandLine.NamingConventionBinder;

using static Cats.Utils.Sheetposter.Extension.SheetExtension;

namespace Cats.Utils.Sheetposter;

public static class CommandFactory
{
    public static Command MakePostCatsTableCommand()
    {
        Command rootCommand = new("postCatsTable", "Post cats contest table to google sheets")
        {
            new Option<int>(new[] { "-c", "--cid" }, "Cats contest id") { IsRequired = true },
            new Option<string>(new[] { "-n", "--name" }, "Google sheets name") { IsRequired = false },
            new Option<bool>(new[] { "-f", "--isFull" }, "Is table full or only final score") { IsRequired = false },
        };
        rootCommand.Handler = CommandHandler.Create<int, string, bool>(static async (cid, name, isFull) =>
        {
            name ??= $"Cats {cid} contest  {DateTime.Now}";
            var catsTable = await CreateCatsTable(cid, isFull);
            Console.WriteLine($"Table url = {(await PostToGoogleDocs(name, catsTable)).SpreadsheetUrl}");
        });

        return rootCommand;
    }
}