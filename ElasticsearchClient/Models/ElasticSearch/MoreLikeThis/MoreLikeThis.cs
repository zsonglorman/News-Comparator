using Newtonsoft.Json;

namespace ElasticsearchClient.Models.ElasticSearch.MoreLikeThis
{
    class MoreLikeThis
    {
        [JsonProperty("like")]
        public Like Like { get; set; }

        [JsonProperty("min_doc_freq")]
        public int MinimumDocumentFrequence { get; set; }
    }
}
