namespace ElasticsearchClient.Models
{
    public class RelatedArticleData
    {
        public bool RelatedArticleExists { get; private set; }

        public string RelatedArticleAddress { get; private set; }

        public RelatedArticleData(bool relatedArticleExists, string relatedArticleAddress)
        {
            RelatedArticleExists = relatedArticleExists;
            RelatedArticleAddress = relatedArticleAddress;
        }
    }
}
