using NLog;
using System;

namespace ArticleCollector
{
    class Program
    {
        /// <summary>
        /// NLog log manager.
        /// </summary>
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Main entry point of application.
        /// </summary>
        static void Main(string[] args)
        {
            Logger.Info("Article Collector started.");

            try
            {                
                // TODO read settings from config file

                // initialize article collector web scraping browser, then get new articles and save them in Elasticsearch
                var articleCollectorBrowser = new WebScraping.ArticleCollectorBrowser();
                articleCollectorBrowser.SaveNewArticlesInElasticsearch().Wait();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error happened while saving new articles in Elasticsearch:");
            }

            Logger.Info("Article Collector now closes.");
        }
    }
}