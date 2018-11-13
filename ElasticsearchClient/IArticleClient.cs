using ElasticsearchClient.Models;
using System.Threading.Tasks;

namespace ElasticsearchClient
{
    public interface IArticleClient
    {
        Task AddArticleAsync(Article article);

        Task<bool> ArticleAlreadyExists(string articleAddress);
    }
}