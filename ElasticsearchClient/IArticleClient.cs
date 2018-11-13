using ElasticsearchClient.Models;
using System.Threading.Tasks;

namespace ElasticsearchClient
{
    public interface IArticleClient
    {
        Task AddArticleAsync(Article article);

        Task<ArticleExistsData> TryGetArticleId(string articleAddress);
    }
}