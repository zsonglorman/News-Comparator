using Newtonsoft.Json;

namespace ElasticsearchClient.Models.Elasticsearch.MoreLikeThis
{
    class Query
    {
        [JsonProperty("more_like_this")]
        public MoreLikeThis MoreLikeThis { get; set; }
    }
}
