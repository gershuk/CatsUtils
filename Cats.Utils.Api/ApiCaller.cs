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

        public async Task<JsonNode?> GetContestTable(int cid)
        {
            var response = await _client.GetAsync($"https://imcs.dvfu.ru/cats/?f=rank_table_content;cid={cid};json=1");
            response.EnsureSuccessStatusCode();
            return JsonNode.Parse(await response.Content.ReadAsStringAsync());
        }

        public async Task<JsonNode?> GetProblems(int cid)
        {
            var response = await _client.GetAsync($"https://imcs.dvfu.ru/cats/?f=problems;cid={cid};json=1;");
            response.EnsureSuccessStatusCode();
            return JsonNode.Parse(await response.Content.ReadAsStringAsync());
        }

        public async Task<JsonNode?> GetTaskProblemInfo(int cpid)
        {
            var response = await _client.GetAsync($"https://imcs.dvfu.ru/cats/?f=problem_text;cpid={cpid};json=1");
            response.EnsureSuccessStatusCode();
            return JsonNode.Parse(await response.Content.ReadAsStringAsync());
        }

        public async Task<JsonNode?> GetTaskProblemInfo(int cid, int pid)
        {
            var response = await _client.GetAsync($"https://imcs.dvfu.ru/cats/?f=problem_text;cid={cid};pid={pid};json=1");
            response.EnsureSuccessStatusCode();
            return JsonNode.Parse(await response.Content.ReadAsStringAsync());
        }
    }
}