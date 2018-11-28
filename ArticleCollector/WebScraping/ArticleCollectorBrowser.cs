using ArticleCollector.WebScraping.NewsWebPortals;
using ElasticsearchClient;
using ElasticsearchClient.Models.News;
using ScrapySharp.Network;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

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
        /// List of web scraping classes for different news portals
        /// </summary>
        private List<ScrapingBase> scrapingTools;

        /// <summary>
        /// Initializes a new web scraping browser for collecting articles.
        /// </summary>
        public ArticleCollectorBrowser()
        {
            // initialize internal web scraping browser
            scrapingBrowser = new ScrapingBrowser()
            {
                // auto detect charset doesn't work, so disable it and set charset manually
                AutoDetectCharsetEncoding = false
            };

            // initialize list of web scraping classes for different news portals
            scrapingTools = new List<ScrapingBase>()
            {
                new IndexScraping(scrapingBrowser),
                new HiradoScraping(scrapingBrowser),
                new OrigoScraping(scrapingBrowser),
            };
        }

        /// <summary>
        /// Collects articles from different news portals, and saves the news ones in Elasticsearch.
        /// </summary>
        public async Task SaveNewArticlesInElasticsearch()
        {
            List<Article> articles;
            try
            {
                articles = GetArticlesFromNewsPortals();
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error happened while getting articles from news portals.", ex);
            }

            // initialize Elasticsearch API client (TODO read URI from config)
            var client = new ElasticsearchArticleClient(new Uri("http://localhost:9200/"));

            foreach (var article in articles)
            {
                try
                {
                    // check whether article already exists in Elasticsearch
                    var articleAlreadyExistsData = await client.TryGetArticleId(article.Address);

                    if (!articleAlreadyExistsData.ArticleExists)
                    {
                        // article doesn't exist in Elasticsearch yet, so let's add it
                        await client.AddArticleAsync(article);
                    }
                    else
                    {
                        // article already exists in Elasticsearch, so we skip this article
                    }
                }
                catch (Exception ex)
                {
                    // TODO log error
                }
            }
        }

        /// <summary>
        /// Retrieves articles from different news portals by web scraping.
        /// </summary>
        /// <returns>list of all articles retrieved</returns>
        private List<Article> GetArticlesFromNewsPortals()
        {
            var allArticles = new List<Article>();

            // get articles from different news portals
            foreach (var scrapingTool in scrapingTools)
            {
                var articles = scrapingTool.GetArticles();
                // merge list of all articles and current articles
                allArticles.AddRange(articles);
            }

            return allArticles;
        }
    }
}