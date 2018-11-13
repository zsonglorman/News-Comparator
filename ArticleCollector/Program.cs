using ElasticsearchClient;
using ElasticsearchClient.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArticleCollector
{
    class Program
    {
        static void Main(string[] args)
        {
            // wait for async main method
            MainAsync().GetAwaiter().GetResult();
        }

        static async Task MainAsync()
        {
            // initialize article collector web scraping browser, and get articles from news portals
            var articleCollectorBrowser = new WebScraping.ArticleCollectorBrowser();
            List<Article> articles = articleCollectorBrowser.GetArticlesFromNewsPortals();

            // initialize Elasticsearch API client (TODO read URI from config)
            var client = new ArticleClient(new Uri("http://localhost:9200/"));

            foreach (var article in articles)
            {
                // check whether article already exists in Elasticsearch
                bool articleAlreadyExists = await client.ArticleAlreadyExists(article);

                if (!articleAlreadyExists)
                {
                    // article doesn't exist yet in Elasticsearch, so let's add it
                    await client.AddArticleAsync(article);
                }
                else
                {
                    // article already exists in Elasticsearch, so we skip this article
                }
            }
        }
    }
}