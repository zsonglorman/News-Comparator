using System;

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

                // initialize article collector web scraping browser, then get new articles and save them in Elasticsearch
                var articleCollectorBrowser = new WebScraping.ArticleCollectorBrowser();
                articleCollectorBrowser.SaveNewArticlesInElasticsearch().Wait();
            }
            catch (Exception ex)
            {
                // TODO log error with complete stacktrace
            }
        }
    }
}