using ArticleCollector.Models;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace ArticleCollector.ElasticsearchApi
{
    /// <summary>
    /// Represents a client used for Elasticsearch API.
    /// </summary>
    class Client
    {
        /// <summary>
        /// The base address of Elasticsearch API.
        /// </summary>
        public Uri BaseAddress { get; private set; }

        /// <summary>
        /// The client used for sending HTTP requests to Elasticsearch API.
        /// </summary>
        private HttpClient client;

        public Client(Uri baseAddress)
        {
            BaseAddress = baseAddress;

            // Microsoft recommends that the HttpClient should be instantiated only once
            // and used for all HTTP requests (to avoid SocketExceptions)
            client = new HttpClient
            {
                BaseAddress = baseAddress
            };

            // set client default Accept header to application/json
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        /// <summary>
        /// Sends the given article to Elasticsearch API.
        /// </summary>
        /// <param name="article">the Article to send</param>
        public async Task AddArticleAsync(Article article)
        {
            HttpResponseMessage response = await client.PostAsJsonAsync("/news/_doc?pretty", article);
            if (response.StatusCode == System.Net.HttpStatusCode.Created)
            {
                // article successfully added
            }
            else
            {
                // article couldn't be added
                string responseContent = await response.Content.ReadAsStringAsync();
                // TODO log error
                throw new ApplicationException("Article couldn't be added to Elasticsearch: " + response.StatusCode.ToString());
            }
        }
    }
}