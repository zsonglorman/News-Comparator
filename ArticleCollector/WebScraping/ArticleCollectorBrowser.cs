using ArticleCollector.WebScraping.NewsWebPortals;
using ElasticsearchClient;
using ElasticsearchClient.Models.News;
using NLog;
using ScrapySharp.Network;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;

namespace ArticleCollector.WebScraping
{
    /// <summary>
    /// Web scraping browser used for collecting and saving news articles.
    /// </summary>
    class ArticleCollectorBrowser
    {
        /// <summary>
        /// NLog log manager.
        /// </summary>
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The browser by ScrapySharp used internally for web scraping.
        /// </summary>
        private ScrapingBrowser scrapingBrowser;

        /// <summary>
        /// List of web scraping classes for different news portals (heterogenous collection).
        /// </summary>
        private List<ScrapingBase> scrapingTools;

        /// <summary>
        /// Initializes a new web scraping browser for collecting and saving news articles.
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

            Logger.Info("Article Collector Browser successfully initialized with {0} scraping tools.", scrapingTools.Count);
        }

        /// <summary>
        /// Collects articles from different news portals, and saves the new ones in Elasticsearch.
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

            // initialize Elasticsearch API client (read base address from config)
            string elasticsearchApiBaseAddress = ConfigurationManager.AppSettings["ElasticsearchApiBaseAddress"];
            if (string.IsNullOrEmpty(elasticsearchApiBaseAddress))
            {
                throw new ApplicationException("ElasticsearchApiBaseAddress is not provided in configuration file!");
            }

            var client = new ElasticsearchArticleClient(new Uri(elasticsearchApiBaseAddress));

            foreach (var article in articles)
            {
                try
                {
                    // check whether article already exists in Elasticsearch
                    var articleAlreadyExistsData = await client.TryGetArticleId(article.Address);

                    if (!articleAlreadyExistsData.ArticleExists)
                    {
                        Logger.Info("This article doesn't exist in Elasticsearch yet, let's save it.");
                        await client.AddArticleAsync(article);
                    }
                    else
                    {
                        Logger.Info("This article already exists in Elasticsearch, so we skip it: {0}", article.Address);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Error happened while handling article {0}:", article.Address);
                }
            }
        }

        /// <summary>
        /// Retrieves articles from different news portals by web scraping.
        /// </summary>
        /// <returns>list of all articles retrieved</returns>
        private List<Article> GetArticlesFromNewsPortals()
        {
            Logger.Info("Getting articles from news portals via web scraping started.");
            var allArticles = new List<Article>();

            // get articles from different news portals
            foreach (var scrapingTool in scrapingTools)
            {
                var articles = scrapingTool.GetArticles();
                // merge list of all articles and current articles
                allArticles.AddRange(articles);
            }

            Logger.Info("Successfully retrieved altogether {0} articles from news portals.", allArticles.Count);
            return allArticles;
        }
    }
}