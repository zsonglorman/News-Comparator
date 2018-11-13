using ElasticsearchClient.Models;
using ElasticsearchClient.Models.Elasticsearch;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace ElasticsearchClient
{
    /// <summary>
    /// Represents a client used for Elasticsearch API.
    /// </summary>
    public class ArticleClient
    {
        /// <summary>
        /// The base address of Elasticsearch API.
        /// </summary>
        public Uri BaseAddress { get; private set; }

        /// <summary>
        /// The client used for sending HTTP requests to Elasticsearch API.
        /// </summary>
        private HttpClient client;

        /// <summary>
        /// Initializes the client with the given Elasticsearch API base address.
        /// </summary>
        /// <param name="baseAddress">the base address of Elasticsearch API</param>
        public ArticleClient(Uri baseAddress)
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
            var response = await client.PostAsJsonAsync("/news/_doc?pretty", article);
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

        /// <summary>
        /// Checks whether the given article already exists in Elasticsearch based on its address.
        /// </summary>
        /// <param name="article">the Article to check</param>
        /// <returns>returns true if the given article exists</returns>
        public async Task<bool> ArticleAlreadyExists(Article article)
        {
            // generate Elasticsearch exists query with the article's address
            var elasticExistsQuery = GetElasticExistsQuery(article);

            // send the query to Elasticsearch API
            var response = await client.PostAsJsonAsync("/news/_search?pretty", elasticExistsQuery);

            // read the string content of the response
            string responseContent = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                // Elasticsearch query was successful, let's get results count from response
                dynamic queryResult = JsonConvert.DeserializeObject<dynamic>(responseContent);
                int? resultsCount = queryResult?.hits?.total;
                // if query result has no hits or total attribute, result will be null

                if (!resultsCount.HasValue)
                {
                    // couldn't get results count from response JSON
                    // TODO log error
                    throw new ApplicationException("Couldn't get results count of Elasticsearch exists query: unexpected response JSON!");
                }

                // return whether there are any results found (0 result: false, 0< results: true)
                return resultsCount.Value > 0;
            }
            else
            {
                // Elasticsearch query was unsuccessful
                // TODO log error
                throw new ApplicationException("Elasticsearch exists query was unsuccessful: " + response.StatusCode.ToString());
            }
        }

        /// <summary>
        /// Generates exists query for Elasticsearch for the given article.
        /// </summary>
        /// <param name="article">query will be generated for this Article</param>
        /// <returns>exists query for Elasticsearch</returns>
        private ExistsQuery GetElasticExistsQuery(Article article)
        {
            var elasticExistsQuery = new ExistsQuery()
            {
                Query = new Query()
                {
                    ConstantScore = new ConstantScore()
                    {
                        Filter = new Filter()
                        {
                            Term = new Term()
                            {
                                AddressKeyword = article.Address
                            }
                        }
                    }
                },
                Source = new List<string>()
                {
                    "Title",
                    "Address"
                }
            };

            return elasticExistsQuery;
        }
    }
}