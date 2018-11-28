using Newtonsoft.Json;

namespace ElasticsearchClient.Models.Elasticsearch.MoreLikeThis
{
    class MoreLikeThisQuery
    {
        [JsonProperty("query")]
        public Query Query { get; set; }
    }
}
