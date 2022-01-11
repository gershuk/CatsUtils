using static Cats.Utils.Api.ApiCaller;
using static Cats.Utils.Sheetposter.Extension.SheetExtension;

namespace Cats.Utils.Sheetposter;

public class Sheetposter
{
    public async static Task Main(string[] args)
    {
        var cid = args.Length >= 1 ? Convert.ToInt32(args[0]) : throw new ArgumentException("Wrong arguments count");
        var name = args.Length == 2 ? args[1] : "Table";

        var catsTable = CreateCatsTable(await GetContestTable(cid)
            ?? throw new NullReferenceException());
        Console.WriteLine((await PostToGoogleDocs(name, catsTable)).SpreadsheetUrl);
    }
}