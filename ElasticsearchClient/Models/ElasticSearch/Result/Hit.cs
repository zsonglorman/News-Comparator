using ElasticsearchClient.Models.News;
using Newtonsoft.Json;

namespace ElasticsearchClient.Models.ElasticSearch.Result
{
    public class Hit
    {
        [JsonProperty("_index")]
        public string Index { get; set; }

        [JsonProperty("_type")]
        public string Type { get; set; }

        [JsonProperty("_id")]
        public string Id { get; set; }

        [JsonProperty("_score")]
        public double Score { get; set; }

        [JsonProperty("_source")]
        public Article Article { get; set; }
    }
}
