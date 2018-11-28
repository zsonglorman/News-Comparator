using ElasticsearchClient.Models.News;
using ScrapySharp.Network;
using System.Collections.Generic;

namespace ArticleCollector.WebScraping.NewsWebPortals
{
    /// <summary>
    /// Serves as an abstract base class for web scraping classes used for retrieving articles from different news portals.
    /// </summary>
    abstract class ScrapingBase
    {
        /// <summary>
        /// The browser by ScrapySharp used internally for web scraping.
        /// </summary>
        protected ScrapingBrowser scrapingBrowser;

        /// <summary>
        /// Initializes a new web scraping object with the given browser.
        /// </summary>
        public ScrapingBase(ScrapingBrowser scrapingBrowser)
        {
            this.scrapingBrowser = scrapingBrowser;
        }

        /// <summary>
        /// Retrieves articles from new portal by web scraping.
        /// </summary>
        /// <returns>list of articles from news portal</returns>
        abstract public List<Article> GetArticles();
    }
}