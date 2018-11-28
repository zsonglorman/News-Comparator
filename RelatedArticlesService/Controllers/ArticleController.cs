﻿using System;
using System.Threading.Tasks;
using ElasticsearchClient;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace RelatedArticlesService.Controllers
{   
    /// <summary>
    /// The main web API controller for news articles.
    /// </summary>
    [Route("api/[controller]")] // specify route to API controller
    public class ArticleController : ControllerBase
    {
        /// <summary>
        /// The article client used internally for finding related articles.
        /// </summary>
        private readonly IArticleClient articleClient;

        /// <summary>
        /// Initializes a new article controller with the given IArticleClient implementation.
        /// </summary>
        public ArticleController(IArticleClient client)
        {
            // use dependency injection to set the client implementation
            articleClient = client;
        }

        /// <summary>
        /// Retrieves a related article (if any) for the given article address.
        /// </summary>
        /// <param name="address">the address to find related articles to, retrieved from URL query string</param>
        /// <returns>the address of the found related article (if any)</returns>
        [HttpGet]
        public async Task<IActionResult> GetRelatedArticleAsync([FromQuery] string address)
        {
            // validate the address retrieved from URL query string
            if (string.IsNullOrWhiteSpace(address))
            {
                return BadRequest("Article address must not be empty.");
            }

            try
            {
                // check whether article already exists in Elasticsearch
                var articleAlreadyExistsData = await articleClient.TryGetArticleId(address);

                if (!articleAlreadyExistsData.ArticleExists)
                {
                    // article doesn't exist in Elasticsearch yet, so return 404 Not Found
                    return NotFound("Article doesn't exist in Elasticsearch yet.");
                }

                // article already exists, get its ID from client response
                string articleId = articleAlreadyExistsData.ArticleId;

                // get related article from Elasticsearch based on article ID
                var relatedArticleData = await articleClient.TryGetRelatedArticleAddress(articleId);
                if (!relatedArticleData.RelatedArticleExists)
                {
                    // there is no related article found, so return 404 Not Found
                    return NotFound("There is no related article in Elasticsearch.");
                }

                // related article successfully found, return its address in the 200 OK response
                return Ok(relatedArticleData.RelatedArticleAddress);
            }
            catch (Exception ex)
            {
                // TODO log error internally
                // return 500 Internal Server Error without exposing details of the error
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error happened while processing the request.");
            }
        }
    }
}