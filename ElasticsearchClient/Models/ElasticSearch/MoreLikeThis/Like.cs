using Newtonsoft.Json;

namespace ElasticsearchClient.Models.ElasticSearch.MoreLikeThis
{
    class Like
    {
        [JsonProperty("_index")]
        public string Index { get; set; }

        [JsonProperty("_type")]
        public string Type { get; set; }

        [JsonProperty("_id")]
        public string Id { get; set; }
    }
}
