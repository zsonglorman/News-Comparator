using ElasticsearchClient.Models.News;
using System.Threading.Tasks;

namespace ElasticsearchClient
{
    /// <summary>
    /// Interface which defines methods for managing the articles between the client and the backend.
    /// </summary>
    public interface IArticleClient
    {
        /// <summary>
        /// Adds the given article to the backend database.
        /// </summary>
        Task AddArticleAsync(Article article);

        /// <summary>
        /// Retrieves article ID based on its address, provided that it already exists in the backend database. 
        /// </summary>
        Task<ArticleExistsData> TryGetArticleId(string articleAddress);

        /// <summary>
        /// Retrieves the address of a related article to the given article ID,
        /// provided that a related article exists in the backend database.
        /// </summary>
        Task<RelatedArticleData> TryGetRelatedArticleAddress(string articleId);
    }
}