using Newtonsoft.Json;

namespace ArticleCollector.Models.Elasticsearch
{
    /// <summary>
    /// Term query finds objects which contain the exact given value.
    /// </summary>
    class Term
    {
        /// <summary>
        /// The term to match in the Address field.
        /// </summary>
        [JsonProperty("Address.keyword")]
        public string AddressKeyword { get; set; }
    }
}