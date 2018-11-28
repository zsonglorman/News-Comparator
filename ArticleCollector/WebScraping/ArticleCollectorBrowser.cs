using ArticleCollector.WebScraping.NewsWebPortals;
using ElasticsearchClient.Models.News;
using ScrapySharp.Network;
using System.Collections.Generic;
using System.Text;

namespace ArticleCollector.WebScraping
{
    /// <summary>
    /// Web scraping browser used for collecting news articles.
    /// </summary>
    class ArticleCollectorBrowser
    {
        /// <summary>
        /// The browser by ScrapySharp used internally for web scraping.
        /// </summary>
        private ScrapingBrowser scrapingBrowser;

        /// <summary>
        /// Initializes a new web scraping browser for collecting articles.
        /// </summary>
        public ArticleCollectorBrowser()
        {
            // initialize internal web scraping browser
            scrapingBrowser = new ScrapingBrowser()
            {
                Encoding = Encoding.UTF8
            };
        }

        /// <summary>
        /// Retrieves articles from different news portals by web scraping.
        /// </summary>
        /// <returns>list of articles retrieved</returns>
        public List<Article> GetArticlesFromNewsPortals()
        {
            // get articles from different news portals
            var allArticles = new IndexScraping(scrapingBrowser).GetArticles();
            var articlesFromHirado = new HiradoScraping(scrapingBrowser).GetArticles();
            var articlesFromOrgio = new OrigoScraping(scrapingBrowser).GetArticles();

            // merge list of articles into one
            allArticles.AddRange(articlesFromHirado);
            allArticles.AddRange(articlesFromOrgio);

            return allArticles;
        }
    }
}