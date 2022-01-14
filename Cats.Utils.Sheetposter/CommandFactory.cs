using System.CommandLine;
using System.CommandLine.NamingConventionBinder;

using static Cats.Utils.Sheetposter.Extension.SheetExtension;

namespace Cats.Utils.Sheetposter;

public static class CommandFactory
{
    public static Command MakeDeleteFileCommand()
    {
        Command command = new("deleteFile", "Delete file from google drive")
        {
            new Option<bool>(new[] { "-s", "--isService" }, "Run as service if true, else as client") { IsRequired = false },
            new Option<string>(new[] { "-i", "--id" }, "File id") { IsRequired = true },
        };
        command.Handler = CommandHandler.Create<bool, string>(async (isService, id) =>
        {
            Poster poster = new(isService);
            Console.WriteLine(await poster.DeleteFile(id));
        });

        return command;
    }

    public static Command MakeExportFildCommand()
    {
        Command command = new("exportFile", "Download file from google drive")
        {
            new Option<bool>(new[] { "-s", "--isService" }, "Is table full or only final score") { IsRequired = false },
            new Option<string>(new[] { "-i", "--id" }, "File id") { IsRequired = true },
            new Option<string>(new[] { "-f", "--format" }, "File export format") { IsRequired = true },
            new Option<string>(new[] { "-p", "--path" }, "File storage path") { IsRequired = false },
        };
        command.Handler = CommandHandler.Create<bool, string, string, string?>(async (isService, id, format, path) =>
        {
            Poster poster = new(isService);
            var state = await poster.ExportFile(id, format, path);
            Console.WriteLine($"Status:{state.Status})");
            Console.WriteLine($"Bytes loaded:{state.BytesDownloaded}");
            if (state.Exception != null)
                Console.WriteLine($"Exception:{state.Exception}");
        });

        return command;
    }

    public static Command MakeGetFilesListCommand()
    {
        Command command = new("list", "Get google drive files list")
        {
            new Option<bool>(new[] { "-s", "--isService" }, "Run as service if true, else as client") { IsRequired = false },
        };
        command.Handler = CommandHandler.Create<bool>(async isService =>
        {
            Poster poster = new(isService);
            var files = await poster.GetFilesList();
            Console.WriteLine("Files:");
            if (files?.Count > 0)
            {
                foreach (var file in files)
                    Console.WriteLine($"{file.Name} (Id:{file.Id}) (Trashed:{file.Trashed})");
            }
            else
            {
                Console.WriteLine("No files found.");
            }
        });

        return command;
    }

    public static Command MakePostCatsTableToSheetsCommand()
    {
        Command command = new("postSheets", "Post cats contest table to google sheets")
        {
            new Option<int>(new[] { "-c", "--cid" }, "Cats contest id") { IsRequired = true },
            new Option<string>(new[] { "-n", "--name" }, "Google sheets name") { IsRequired = false },
            new Option<bool>(new[] { "-f", "--isFull" }, "Is table full or only final score") { IsRequired = false },
            new Option<bool>(new[] { "-s", "--isService" }, "Run as service if true, else as client") { IsRequired = false },
        };
        command.Handler = CommandHandler.Create<int, string, bool, bool>(static async (cid, name, isFull, isService) =>
        {
            Poster poster = new(isService);
            name ??= $"Cats {cid} contest  {DateTime.Now}";
            var catsTable = await CreateCatsTable(cid, isFull);
            Console.WriteLine($"Table url = {(await poster.PostToGoogleDocs(name, catsTable)).SpreadsheetUrl}");
        });

        return command;
    }

    [Obsolete]
    public static Command MakeTrashFileCommand()
    {
        Command command = new("trashFile", "Moves a file to the trash (used old api version)")
        {
            new Option<bool>(new[] { "-s", "--isService" }, "Run as service if true, else as client") { IsRequired = false },
            new Option<string>(new[] { "-i", "--id" }, "File id") { IsRequired = true },
        };
        command.Handler = CommandHandler.Create<bool, string>(async (isService, id) =>
        {
            Poster poster = new(isService);
            Console.WriteLine(await poster.TrashFile(id));
        });

        return command;
    }

    [Obsolete]
    public static Command MakeUntrashFileCommand()
    {
        Command command = new("unstrashFile", "Restores a file from the trash (used old api version)")
        {
            new Option<bool>(new[] { "-s", "--isService" }, "Is table full or only final score") { IsRequired = false },
            new Option<string>(new[] { "-i", "--id" }, "File id") { IsRequired = true },
        };
        command.Handler = CommandHandler.Create<bool, string>(async (isService, id) =>
        {
            Poster poster = new(isService);
            Console.WriteLine(await poster.TrashFile(id));
        });

        return command;
    }
}