namespace ElasticsearchClient.Models
{
    /// <summary>
    /// Represents the response object of a related article (More Like This) query.
    /// </summary>
    public class RelatedArticleData
    {
        /// <summary>
        /// Indicates whether a related article was found in Elasticsearch.
        /// </summary>
        public bool RelatedArticleExists { get; private set; }

        /// <summary>
        /// The address of a related article found in Elasticsearch.
        /// </summary>
        public string RelatedArticleAddress { get; private set; }

        /// <summary>
        /// Initializes a new object with the given parameters.
        /// </summary>
        public RelatedArticleData(bool relatedArticleExists, string relatedArticleAddress)
        {
            RelatedArticleExists = relatedArticleExists;
            RelatedArticleAddress = relatedArticleAddress;
        }
    }
}