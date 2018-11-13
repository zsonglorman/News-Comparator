﻿using ElasticsearchClient.Models;
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
    public class ElasticsearchArticleClient : IArticleClient
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
        public ElasticsearchArticleClient(Uri baseAddress)
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
        /// Returns article ID based on its address, if the article already exists in Elasticsearch.
        /// </summary>
        /// <param name="articleAddress">the article address to check</param>
        /// <returns>returns true if the given article exists together with its id</returns>
        public async Task<ArticleExistsData> TryGetArticleId(string articleAddress)
        {
            // generate Elasticsearch exists query with the article's address
            var elasticExistsQuery = GetElasticExistsQuery(articleAddress);

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

                // check whether there are any results found (0 result: false, 0< results: true)
                bool articleExists = resultsCount.Value > 0;
                
                string articleId = "";
                if (articleExists)
                {
                    // if there is a result, get its id, and include it in the return data 
                    articleId = queryResult?.hits?.hits?[0]._id;
                }

                return new ArticleExistsData(articleExists, articleId);
            }
            else
            {
                // Elasticsearch query was unsuccessful
                // TODO log error
                throw new ApplicationException("Elasticsearch exists query was unsuccessful: " + response.StatusCode.ToString());
            }
        }

        /// <summary>
        /// Generates exists query for Elasticsearch for the given article address.
        /// </summary>
        /// <param name="articleAddress">query will be generated for this article address</param>
        /// <returns>exists query for Elasticsearch</returns>
        private ExistsQuery GetElasticExistsQuery(string articleAddress)
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
                                AddressKeyword = articleAddress
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