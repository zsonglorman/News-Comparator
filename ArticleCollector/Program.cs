using ElasticsearchClient;
using ElasticsearchClient.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArticleCollector
{
    class Program
    {
        /// <summary>
        /// Main entry point of application.
        /// </summary>
        static void Main(string[] args)
        {
            try
            {
                // TODO read settings from config file

                // wait for async main method
                MainAsync().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                // TODO log error with complete stacktrace
            }
        }

        /// <summary>
        /// Main async method of application.
        /// </summary>
        static async Task MainAsync()
        {
            List<Article> articles;
            try
            {
                // initialize article collector web scraping browser, and get articles from news portals
                var articleCollectorBrowser = new WebScraping.ArticleCollectorBrowser();
                articles = articleCollectorBrowser.GetArticlesFromNewsPortals();
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
    }
}