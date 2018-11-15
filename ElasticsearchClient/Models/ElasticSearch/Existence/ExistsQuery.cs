using Newtonsoft.Json;
using System.Collections.Generic;

namespace ElasticsearchClient.Models.Elasticsearch.Existence
{
    /// <summary>
    /// Represents an Elasticsearch query which checks whether an object already exists in the index.
    /// </summary>
    class ExistsQuery
    {
        /// <summary>
        /// The query which will be sent to Elasticsearch API.
        /// </summary>
        [JsonProperty("query")]
        public Query Query { get; set; }

        /// <summary>
        /// List of fields which will be returned by the query.
        /// </summary>
        [JsonProperty("_source")]
        public List<string> Source { get; set; }
    }
}