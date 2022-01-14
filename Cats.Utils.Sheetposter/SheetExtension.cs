using Cats.Utils.Api;

using Google.Apis.Sheets.v4.Data;

using System.Text.Json.Nodes;

namespace Cats.Utils.Sheetposter.Extension;

public static partial class SheetExtension
{
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

    public static async Task<Sheet> CreateCatsTable(int cid, bool isFull = true)
    {
        using var apiCaller = new ApiCaller();
        var table = await apiCaller.GetContestTable(cid) ?? throw new NullReferenceException();
        var problemIds = (JsonArray)(table["problem_ids"] ?? throw new NullReferenceException());
        var ranks = (JsonArray)(table["ranks"] ?? throw new NullReferenceException());
        var width = isFull ? problemIds.Count + 2 : 2;
        var height = ranks.Count + 2;
        var sheet = Create(width, height);

        sheet.Set(0, 0, new() { StringValue = "Имя" });
        sheet.Set(isFull ? problemIds.Count + 1 : 1, 0, new() { StringValue = "Результат" });

        if (isFull)
        {
            for (var i = 0; i < problemIds.Count; ++i)
            {
                var pid = problemIds[i][0].ToString();
                var taskNode = (await apiCaller.GetTaskProblemInfo(cid, Convert.ToInt32(pid)));
                if (((JsonArray)taskNode).Count == 0)
                    continue;
                taskNode = taskNode[0];
                sheet.Set(i + 1, 0, new()
                {
                    FormulaValue = MakeHyperLink($"https://imcs.dvfu.ru/cats/?f=problem_text;cid={cid};pid={pid};",
                                                taskNode["title"].ToString())
                });
            }
        }

        for (var i = 0; i < ranks.Count; ++i)
        {
            var pt = (JsonArray)ranks[i]["pt"] ?? (JsonArray)ranks[i]["td"] ?? throw new NullReferenceException();
            sheet.Set(0, i + 1, new() { StringValue = ranks[i]["n"].ToString() });
            sheet.Set(isFull ? pt.Count + 1 : 1, i + 1, new() { StringValue = (ranks[i]["tp"] ?? ranks[i]["ts"]).ToString() });

            if (isFull)
            {
                for (var j = 0; j < pt.Count; ++j)
                    sheet.Set(j + 1, i + 1, new() { StringValue = pt[j].ToString() });
            }
        }

        return sheet;
    }

    public static string MakeHyperLink(string link, string? text = default) =>
        $"=ГИПЕРССЫЛКА(\"{link}\";\"{text ?? link}\")";

    #region GettersSetters

    public static CellData Get(this Sheet sheet, int width, int height) => sheet.Data[0].RowData[height].Values[width];

    public static void Set(this Sheet sheet, int width, int height, ExtendedValue value) =>
                sheet.Data[0].RowData[height].Values[width].UserEnteredValue = value;

    #endregion GettersSetters
}