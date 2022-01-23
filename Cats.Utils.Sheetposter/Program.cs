using System.CommandLine;

using static Cats.Utils.Sheetposter.CommandFactory;

namespace Cats.Utils.Sheetposter
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var rootCommand = new RootCommand();
            var commandMakerList = new Func<Command>[]
            {
                MakePostCatsTableToSheetsCommand,
                MakeGetFilesListCommand,
                MakeDeleteFileCommand,
                MakeTrashFileCommand,
                MakeUntrashFileCommand,
                MakeExportFildCommand,
                MakeLoginToCatsCommand,
            };

            foreach (var commandMaker in commandMakerList)
                rootCommand.Add(commandMaker());
            await rootCommand.InvokeAsync(args);
        }
    }
}