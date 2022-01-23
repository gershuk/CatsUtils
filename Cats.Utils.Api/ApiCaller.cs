using System.Text.Json.Nodes;

namespace Cats.Utils.Api
{
    public sealed partial class ApiCaller : IDisposable
    {
        private readonly HttpClient _client = new();
        private bool _disposed = false;

        #region Dispose Pattern Realization

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _client.Dispose();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion Dispose Pattern Realization

        private async Task<JsonNode?> Get(string url)
        {
            using var response = await _client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            return JsonNode.Parse(await response.Content.ReadAsStringAsync());
        }

        private async Task<JsonNode?> Post(string url, HttpContent content)
        {
            using var response = await _client.PostAsync(url, content);
            response.EnsureSuccessStatusCode();
            return JsonNode.Parse(await response.Content.ReadAsStringAsync());
        }

        public async Task<JsonNode?> GetContestTable(int cid) => await Get($"https://imcs.dvfu.ru/cats/?f=rank_table_content;cid={cid};json=1");

        public async Task<JsonNode?> GetProblems(int cid) => await Get($"https://imcs.dvfu.ru/cats/?f=problems;cid={cid};json=1;");

        public async Task<JsonNode?> GetTaskProblemInfo(int cpid) => await Get($"https://imcs.dvfu.ru/cats/?f=problem_text;cpid={cpid};json=1");

        public async Task<JsonNode?> GetTaskProblemInfo(int cid, int pid) => await Get($"https://imcs.dvfu.ru/cats/?f=problem_text;cid={cid};pid={pid};json=1");

        public async Task<JsonNode?> Login(string login, string password) =>
            await Post($"https://imcs.dvfu.ru/cats/?f=login;json=1",
                        new FormUrlEncodedContent(new Dictionary<string, string>()
                        {
                            { "login", login },
                            { "passwd", password },
                            { "submit", "1" }
                        }));

        public async Task<JsonNode?> PostTaskToCats(string sid, int cid, string text, string deId, int problemId) =>
            await Post($"https://imcs.dvfu.ru/cats/?f=problems;sid={sid};cid={cid};json=1",
                        new FormUrlEncodedContent(new Dictionary<string, string>()
                        {
                            {"problem_id", problemId.ToString()},
                            {"de_id", deId },
                            {"source_text", text },
                            {"submitted", "1" },
                            {"submit", "1" },
                        }));
    }
}