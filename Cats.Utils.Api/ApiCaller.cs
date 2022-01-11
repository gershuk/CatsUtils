using System.Text.Json.Nodes;

namespace Cats.Utils.Api
{
    public static partial class ApiCaller
    {
        public static async Task<JsonNode?> GetContestTable(int cid)
        {
            using HttpClient client = new();
            var response = await client.GetAsync($"https://imcs.dvfu.ru/cats/?f=rank_table_content;cid={cid};json=1");
            response.EnsureSuccessStatusCode();
            return JsonNode.Parse(await response.Content.ReadAsStringAsync());
        }
    }
}