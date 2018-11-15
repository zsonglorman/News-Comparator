namespace ElasticsearchClient.Models.News
{
    /// <summary>
    /// Represents the response object of an exists query.
    /// </summary>
    public class ArticleExistsData
    {
        /// <summary>
        /// Indicates whether the article already exists in Elasticsearch.
        /// </summary>
        public bool ArticleExists { get; private set; }

        /// <summary>
        /// The ID of the article in Elasticsearch.
        /// </summary>
        public string ArticleId { get; private set; }

        /// <summary>
        /// Initializes a new object with the given parameters.
        /// </summary>
        public ArticleExistsData(bool articleExists, string articleId)
        {
            ArticleExists = articleExists;
            ArticleId = articleId;
        }
    }
}