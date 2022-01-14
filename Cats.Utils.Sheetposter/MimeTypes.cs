using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cats.Utils.Sheetposter
{
    public record struct MimeType(string Format, string Mime);

    public record class MimeTypesMapper(MimeType[] AppsScripts, MimeType[] Documents, MimeType[] Drawings, MimeType[] Presentations, MimeType[] Spreadsheets)
    {
        public static MimeTypesMapper LoadFromFile(string path) =>
            JsonSerializer.Deserialize<MimeTypesMapper>(File.ReadAllText(path));

        public static void SaveToFile(string path, MimeTypesMapper mimeTypesMappers) =>
            File.WriteAllText(path, JsonSerializer.Serialize(mimeTypesMappers));
    }

    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(MimeTypesMapper))]
    public partial class SourceGenerationContext : JsonSerializerContext
    {
    }

    public enum AppsScripts
    {
        JSON,
    }

    public enum ConversionFormatDocuments
    {
        HTML,
        HTMLZ, //HTML zipped
        PlainText,
        RichText,
        OpenOfficeDoc,
        PDF,
        MSWordDocument,
        EPUB
    }

    public enum ConversionFormatSpreadsheets
    {
        MSExcel,
        OpenOfficeSheet,
        PDF,
        CSV, //first sheet only
        SHO, // sheet only
        HTMLZ, //HTML zipped
    }

    public enum Presentations
    {
        MSPowerPoint,
        OpenOfficePresentation,
        PDF,
        PlainText,
    }
}