using Newtonsoft.Json;

namespace ElasticsearchClient.Models.Elasticsearch
{
    /// <summary>
    /// Defines filter context clause for Elasticsearch (used for filtering structured data).
    /// </summary>
    class Filter
    {
        /// <summary>
        /// Term query finds objects which contain the exact given value.
        /// </summary>
        [JsonProperty("term")]
        public Term Term { get; set; }
    }
}