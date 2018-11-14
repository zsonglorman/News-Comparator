using Newtonsoft.Json;

namespace ElasticsearchClient.Models.ElasticSearch.MoreLikeThis
{
    class Query
    {
        [JsonProperty("more_like_this")]
        public MoreLikeThis MoreLikeThis { get; set; }
    }
}
