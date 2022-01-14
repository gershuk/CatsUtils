using Google.Apis.Auth.OAuth2;
using Google.Apis.Download;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Util.Store;

using System.Net;
using System.Net.Http.Headers;

namespace Cats.Utils.Sheetposter
{
    public sealed class Poster
    {
        private const string _defaultClientAppKeysPath = "clientAppKeys.json";
        private const string _defaultServiceAppKeysPath = "serviceAppKeys.json";
        private static readonly MimeTypesMapper _mapper = MimeTypesMapper.LoadFromFile("MimeTypes.json");
        private readonly string _appName = "Cats Sheetposter";
        private readonly string _credPath = "token";
        private readonly string[] _scopes = { SheetsService.Scope.Spreadsheets, SheetsService.Scope.Drive };

        public string AppKeysPath { get; init; }

        public bool IsService { get; init; }

        public Poster(bool isService = false, string? appKeysPath = default)
        {
            IsService = isService;
            AppKeysPath = appKeysPath ?? (IsService ? _defaultServiceAppKeysPath : _defaultClientAppKeysPath);
        }

        /// <summary>
        /// https://www.daimto.com/google-drive-downloading-large-files-with-c/
        /// </summary>
        private async Task DownloadLargFile(string id, string? path = default)
        {
            using var service = await MakeService<DriveService>();

            const int KB = 0x400;
            var chunkSize = 256 * KB; // 256KB;

            var fileRequest = service.Files.Get(id);
            fileRequest.Fields = "*";
            var fileResponse = fileRequest.Execute();

            var exportRequest = service.Files.Export(fileResponse.Id, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            var client = exportRequest.Service.HttpClient;

            //you would need to know the file size
            var size = fileResponse.Size;

            await using var file = new FileStream(fileResponse.Name, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            file.SetLength((long)size);

            var chunks = (size / chunkSize) + 1;
            for (long index = 0; index < chunks; index++)
            {
                var request = exportRequest.CreateRequest();

                var from = index * chunkSize;
                var to = @from + chunkSize - 1;

                request.Headers.Range = new RangeHeaderValue(@from, to);

                var response = await client.SendAsync(request);

                if (response.StatusCode != HttpStatusCode.PartialContent && !response.IsSuccessStatusCode)
                    continue;

                await using var stream = await response.Content.ReadAsStreamAsync();
                file.Seek(@from, SeekOrigin.Begin);
                await stream.CopyToAsync(file);
            }
        }

        private async Task<T> MakeService<T>() where T : BaseClientService, new()
        {
            using var stream = File.OpenRead(AppKeysPath);
            var baseClient = IsService ?
                new BaseClientService.Initializer()
                {
                    HttpClientInitializer = GoogleCredential.FromStream(stream).CreateScoped(DriveService.Scope.Drive),
                    ApplicationName = _appName,
                } :
                new BaseClientService.Initializer()
                {
                    HttpClientInitializer = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.FromStream(stream).Secrets,
                    _scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(_credPath, true)),
                    ApplicationName = _appName,
                };
            return (T)(typeof(T).GetConstructor(new[] { typeof(BaseClientService.Initializer) })?.Invoke(new[] { baseClient })
                ?? throw new NullReferenceException());
        }

        public async Task<string> DeleteFile(string id)
        {
            using var service = await MakeService<DriveService>();
            return await service.Files.Delete(id).ExecuteAsync();
        }

        public async Task<IDownloadProgress> ExportFile(string id, string format, string? path = default)
        {
            using var service = await MakeService<DriveService>();

            var fileRequest = service.Files.Get(id);
            fileRequest.Fields = "*";
            var fileResponse = fileRequest.Execute();

            //ToDo : Make auto format detector
            var exportRequest = service.Files.Export(fileResponse.Id, format);
            using var file = new FileStream(path ?? fileResponse.Name, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            return await exportRequest.DownloadAsync(file);
        }

        public async Task<IList<Google.Apis.Drive.v3.Data.File>> GetFilesList()
        {
            using var service = await MakeService<DriveService>();
            var listRequest = service.Files.List();
            listRequest.Fields = "nextPageToken, files(id, name, trashed)";
            return (await listRequest.ExecuteAsync()).Files;
        }

        public async Task<Spreadsheet> PostToGoogleDocs(string name, params Sheet[] sheets)
        {
            using var service = await MakeService<SheetsService>();
            return await (service).Spreadsheets.Create(new()
            {
                Properties = new SpreadsheetProperties() { Title = name },
                Sheets = sheets,
            }).ExecuteAsync();
        }

        [Obsolete]
        public async Task<Google.Apis.Drive.v2.Data.File> TrashFile(string id)
        {
            using var service = await MakeService<Google.Apis.Drive.v2.DriveService>();
            return await service.Files.Trash(id).ExecuteAsync();
        }

        [Obsolete]
        public async Task<Google.Apis.Drive.v2.Data.File> UntrashFile(string id)
        {
            using var service = await MakeService<Google.Apis.Drive.v2.DriveService>();
            return await service.Files.Untrash(id).ExecuteAsync();
        }
    }
}