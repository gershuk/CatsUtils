using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Sheets.v4;
using Google.Apis.Util.Store;
using System.Text.Json.Nodes;

namespace Cats.Utils.Sheetposter.Extension;

public static partial class SheetExtension
{
    static readonly string[] _scopes = { SheetsService.Scope.Spreadsheets, SheetsService.Scope.Drive };
    static readonly string _appName = "Cats Sheetposter";
    static readonly string _credPath = "token.json";
    static readonly string _appKeysPath = "appKeys.json";

    public static CellData Get(this Sheet sheet, int width, int height) => sheet.Data[0].RowData[height].Values[width];
    public static void Set(this Sheet sheet, int width, int height, ExtendedValue value) =>
        sheet.Data[0].RowData[height].Values[width].UserEnteredValue = value;

    public static Sheet Create(int width, int height)
    {
        var rowData = new RowData[height];
        for (var h = 0; h < height; ++h)
        {
            rowData[h] = new RowData() { Values = new CellData[width] };
            for (var w = 0; w < width; ++w)
                rowData[h].Values[w] = new CellData() { UserEnteredValue = new ExtendedValue() };
        }
        return new() { Data = new List<GridData>() { new GridData() { RowData = rowData } } };
    }

    public static Sheet CreateCatsTable(JsonNode node) => throw new NotImplementedException();

    public static async Task<Spreadsheet> PostToGoogleDocs(string name, params Sheet[] sheets)
    {
        using FileStream stream = File.OpenRead(_appKeysPath);

        var service = new SheetsService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                GoogleClientSecrets.FromStream(stream).Secrets,
                _scopes,
                "user",
                CancellationToken.None,
                new FileDataStore(_credPath, true)),
            ApplicationName = _appName,
        });

        Spreadsheet spreadsheet = new()
        {
            Properties = new SpreadsheetProperties() { Title = name },
            Sheets = sheets,
        };

        return await service.Spreadsheets.Create(spreadsheet).ExecuteAsync();
    }
}