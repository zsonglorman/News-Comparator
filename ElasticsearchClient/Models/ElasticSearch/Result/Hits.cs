using Newtonsoft.Json;
using System.Collections.Generic;

namespace ElasticsearchClient.Models.Elasticsearch.Result
{
    public class Hits
    {
        [JsonProperty("total")]
        public int Total { get; set; }

        [JsonProperty("max_score")]
        public double? MaxScore { get; set; }

        [JsonProperty("hits")]
        public List<Hit> HitList { get; set; }
    }
}