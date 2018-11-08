﻿using Newtonsoft.Json;

namespace ArticleCollector.Models.ElasticSearch
{
    /// <summary>
    /// Constant score tells Elasticsearch that scoring of results should be disabled.
    /// </summary>
    class ConstantScore
    {
        /// <summary>
        /// Defines filter context clause for Elasticsearch (used for filtering structured data).
        /// </summary>
        [JsonProperty("filter")]
        public Filter Filter { get; set; }
    }
}