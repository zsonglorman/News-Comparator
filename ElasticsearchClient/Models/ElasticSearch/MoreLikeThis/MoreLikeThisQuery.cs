using Newtonsoft.Json;

namespace ElasticsearchClient.Models.ElasticSearch.MoreLikeThis
{
    class MoreLikeThisQuery
    {
        [JsonProperty("query")]
        public Query Query { get; set; }
    }
}
