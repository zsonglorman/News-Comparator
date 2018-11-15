using Newtonsoft.Json;

namespace ElasticsearchClient.Models.Elasticsearch.Existence
{
    /// <summary>
    /// The query which will be sent to Elasticsearch API.
    /// </summary>
    class Query
    {
        /// <summary>
        /// Constant score tells Elasticsearch that scoring of results should be disabled.
        /// </summary>
        [JsonProperty("constant_score")]
        public ConstantScore ConstantScore { get; set; }
    }
}