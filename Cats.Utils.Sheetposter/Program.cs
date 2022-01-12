using System.CommandLine;

using static Cats.Utils.Sheetposter.CommandFactory;

namespace Cats.Utils.Sheetposter
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var rootCommand = new RootCommand();
            rootCommand.Add(MakePostCatsTableCommand());
            await rootCommand.InvokeAsync(args);
        }
    }
}