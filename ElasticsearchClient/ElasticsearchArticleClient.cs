﻿using ElasticsearchClient.Models.Elasticsearch.Existence;
using ElasticsearchClient.Models.Elasticsearch.MoreLikeThis;
using ElasticsearchClient.Models.Elasticsearch.Result;
using ElasticsearchClient.Models.News;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace ElasticsearchClient
{
    /// <summary>
    /// Represents a client used for managing articles via Elasticsearch API.
    /// </summary>
    public class ElasticsearchArticleClient : IArticleClient
    {
        /// <summary>
        /// NLog log manager.
        /// </summary>
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

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

            Logger.Info("Elasticsearch Article Client successfully initialized with base address {0}", baseAddress.AbsoluteUri);
        }

        /// <summary>
        /// Sends the given article to Elasticsearch API.
        /// </summary>
        /// <param name="article">the Article to send</param>
        public async Task AddArticleAsync(Article article)
        {
            Logger.Info("Saving article in Elasticsearch started: {0}", article.Address);

            var response = await client.PostAsJsonAsync("/news/_doc?pretty", article);
            if (response.StatusCode == System.Net.HttpStatusCode.Created)
            {
                Logger.Info("Article successfully saved in Elasticsearch.");
            }
            else
            {
                // Elasticsearch API returned an error
                string responseContent = await response.Content.ReadAsStringAsync();
                Logger.Error("Article couldn't be saved in Elasticsearch: {0} - {1}", response.StatusCode.ToString(), responseContent);

                throw new ApplicationException("Article couldn't be added to Elasticsearch: " + response.StatusCode.ToString());
            }
        }

        /// <summary>
        /// Retrieves article ID based on its address, if the article already exists in Elasticsearch.
        /// </summary>
        /// <param name="articleAddress">the article address to check</param>
        /// <returns>returns true if the given article exists together with its ID</returns>
        public async Task<ArticleExistsData> TryGetArticleId(string articleAddress)
        {
            Logger.Info("Getting article ID started: {0}", articleAddress);

            // generate Elasticsearch exists query with the article's address
            var elasticExistsQuery = GetElasticExistsQuery(articleAddress);

            // send the query to Elasticsearch API
            var response = await client.PostAsJsonAsync("/news/_search?pretty", elasticExistsQuery);

            // read the string content of the response
            string responseContent = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                Logger.Info("Elasticsearch exists query was successful.");

                // Elasticsearch query was successful, deserialize result
                var existsQueryResult = JsonConvert.DeserializeObject<QueryResult>(responseContent);

                if (existsQueryResult == null || existsQueryResult.Hits == null
                    || existsQueryResult.Hits.HitList == null)
                {
                    Logger.Error("Elasticsearch returned unexpected JSON: {0}", responseContent);
                    throw new ApplicationException("Couldn't get result of Elasticsearch exists query: unexpected response JSON!");
                }

                // check whether there are any results found (0 result: false, 0< results: true)
                bool articleExists = existsQueryResult.Hits.Total > 0;
                
                string articleId = "";
                if (articleExists)
                {
                    // if there is a result, get its id, and include it in the return data
                    articleId = existsQueryResult.Hits.HitList[0].Id;
                }

                Logger.Info("Article exists: {0}, with ID: {1}", articleExists, articleId);
                return new ArticleExistsData(articleExists, articleId);
            }
            else
            {
                // Elasticsearch query was unsuccessful
                Logger.Error("Elasticsearch exists query was unsuccessful: {0} - {1}", response.StatusCode.ToString(), responseContent);
                throw new ApplicationException("Elasticsearch exists query was unsuccessful: " + response.StatusCode.ToString());
            }
        }

        /// <summary>
        /// Generates Exists query for Elasticsearch for the given article address.
        /// </summary>
        /// <param name="articleAddress">query will be generated for this article address</param>
        /// <returns>Exists query for Elasticsearch</returns>
        private ExistsQuery GetElasticExistsQuery(string articleAddress)
        {
            var elasticExistsQuery = new ExistsQuery()
            {
                Query = new Models.Elasticsearch.Existence.Query()
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

        /// <summary>
        /// Retrieves the address of a related article to the given article ID from Elasticsearch.
        /// </summary>
        /// <param name="articleId">the article ID to search for related article</param>
        /// <returns>returns true if the related article exists together with its address</returns>
        public async Task<RelatedArticleData> TryGetRelatedArticleAddress(string articleId)
        {
            Logger.Info("Getting related article's address started for article ID: {0}", articleId);

            // generate Elasticsearch more like this query with the article's id
            var elasticMoreLikeThisQuery = GetElasticMoreLikeThisQuery(articleId);

            // send the query to Elasticsearch API
            var response = await client.PostAsJsonAsync("/news/_search?pretty", elasticMoreLikeThisQuery);

            // read the string content of the response
            string responseContent = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                Logger.Info("Elasticsearch More Like This query was successful.");

                // Elasticsearch query was successful, deserialize result
                var moreLikeThisResult = JsonConvert.DeserializeObject<QueryResult>(responseContent);

                if (moreLikeThisResult == null || moreLikeThisResult.Hits == null
                    || moreLikeThisResult.Hits.HitList == null)
                {
                    Logger.Error("Elasticsearch returned unexpected JSON: {0}", responseContent);
                    throw new ApplicationException("Couldn't get result of Elasticsearch more like this query: unexpected response JSON!");
                }

                string relatedArticleAddress = "";
                if (moreLikeThisResult.Hits.HitList.Count == 0)
                {
                    // there is no related article at all
                    Logger.Info("There is related article found at all.");
                    return new RelatedArticleData(false, relatedArticleAddress);
                }

                // get most related article from hits list
                var mostRelatedArticleResult = moreLikeThisResult.Hits.HitList[0];
                if (mostRelatedArticleResult.Score < 10)
                {
                    // based on the result score, there is no related article
                    Logger.Info("There is no related article found based on Elasticsearch score.");
                    return new RelatedArticleData(false, relatedArticleAddress);
                }

                // return the most related article's address
                Logger.Info("Related article found: {0}", mostRelatedArticleResult.Article.Address);
                return new RelatedArticleData(true, mostRelatedArticleResult.Article.Address);
            }
            else
            {
                // Elasticsearch query was unsuccessful
                Logger.Error("Elasticsearch More Like This query was unsuccessful: {0} - {1}",
                    response.StatusCode.ToString(), responseContent);
                throw new ApplicationException("Elasticsearch More Like This query was unsuccessful: " + response.StatusCode.ToString());
            }
        }

        /// <summary>
        /// Generates More Like This query for Elasticsearch for the given article ID.
        /// </summary>
        /// <param name="articleId">query will be generated for this article ID</param>
        /// <returns>More Like This query for Elasticsearch</returns>
        private MoreLikeThisQuery GetElasticMoreLikeThisQuery(string articleId)
        {
            var elasticMoreLikeThisQuery = new MoreLikeThisQuery()
            {
                Query = new Models.Elasticsearch.MoreLikeThis.Query()
                {
                    MoreLikeThis = new MoreLikeThis()
                    {
                        Like = new Like()
                        {
                            Index = "news",
                            Type = "_doc",
                            Id = articleId
                        },
                        MinimumDocumentFrequence = 1
                    }
                }
            };

            return elasticMoreLikeThisQuery;
        }
    }
}