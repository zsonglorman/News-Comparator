namespace ElasticsearchClient.Models.News
{
    /// <summary>
    /// Represents a simple news article of an online newspaper.
    /// </summary>
    public class Article
    {
        /// <summary>
        /// The URL of the news article.
        /// </summary>
        public string Address { get; private set; }

        /// <summary>
        /// The title (headline) of the news article.
        /// </summary>
        public string Title { get; private set; }

        /// <summary>
        /// The short intro of the news article.
        /// </summary>
        public string Lead { get; private set; }

        /// <summary>
        /// The basic text content of the news article.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Initializes a news article with the given parameters.
        /// </summary>
        public Article(string address, string title, string lead)
        {
            Address = address;
            Title = title;
            Lead = lead;
            // Text attribute will be set later when the scraping browser navigates to the main page of the news article 
        }
    }
}