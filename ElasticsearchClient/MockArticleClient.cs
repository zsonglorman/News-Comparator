using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ElasticsearchClient.Models.News;

namespace ElasticsearchClient
{
    /// <summary>
    /// Represents a mock implementation of a client managing the articles.
    /// </summary>
    public class MockArticleClient : IArticleClient
    {
        /// <summary>
        /// List of articles will be stored in memory.
        /// </summary>
        private readonly List<Article> articles;

        /// <summary>
        /// Random number generator used for returning a randomly chosen article from the list.
        /// </summary>
        private static Random random = new Random();

        /// <summary>
        /// Initializes the mock client with some test articles. 
        /// </summary>
        public MockArticleClient()
        {
            articles = new List<Article>()
            {
                new Article("https://index.hu/belfold/test-article", "Index title", "Index lead"),
                new Article("https://www.hirado.hu/belfold/test-article", "Hirado title", "Hirado lead"),
                new Article("http://www.origo.hu/itthon/test-article", "Origo title", "Origo lead")
            };
        }

        /// <summary>
        /// Adds the given article to the list of articles stored in memory.
        /// </summary>
        public Task AddArticleAsync(Article article)
        {
            articles.Add(article);

            // mimic async behavior by returning a completed task
            return Task.CompletedTask;
        }

        /// <summary>
        /// Retrieves an article based on its address (if it already exists), and returns its random ID.
        /// </summary>
        public Task<ArticleExistsData> TryGetArticleId(string articleAddress)
        {
            var article = articles.Find(a => a.Address == articleAddress);
            if (article == null)
            {
                // no such article found
                return Task.FromResult(new ArticleExistsData(false, ""));
            }

            // article found, return a random ID (GUID) for it
            return Task.FromResult(new ArticleExistsData(true, Guid.NewGuid().ToString()));
        }

        /// <summary>
        /// Returns a random article's address from memory.
        /// </summary>
        public Task<RelatedArticleData> TryGetRelatedArticleAddress(string articleId)
        {
            // generate random index and return the randomly chosen article's address
            int randomIndex = random.Next(articles.Count);
            return Task.FromResult(new RelatedArticleData(true, articles[randomIndex].Address));
        }
    }
}