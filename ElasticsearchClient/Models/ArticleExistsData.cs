namespace ElasticsearchClient.Models
{
    public class ArticleExistsData
    {
        public bool ArticleExists { get; private set; }

        public string ArticleId { get; private set; }

        public ArticleExistsData(bool articleExists, string articleId)
        {
            ArticleExists = articleExists;
            ArticleId = articleId;
        }
    }
}