using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ElasticsearchClient;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace RelatedArticlesService.Controllers
{
    // specify route to API controller
    [Route("api/[controller]")]
    public class ArticleController : Controller
    {
        private readonly IArticleClient elasticsearchClient;

        public ArticleController(IArticleClient client)
        {
            elasticsearchClient = client; 
        }

        public async Task<IActionResult> Get([FromQuery] string address)
        {
            if (string.IsNullOrWhiteSpace(address))
            {
                return BadRequest("Article address must not be empty.");
            }

            // check whether article already exists in Elasticsearch
            bool articleAlreadyExists = await elasticsearchClient.ArticleAlreadyExists(address);

            if (!articleAlreadyExists)
            {
                // article doesn't exist yet in Elasticsearch, so return 404 Not Found
                return NotFound("Article doesn't exist in Elasticsearch yet.");
            }

            // TODO
            return Ok(address);
        }
    }
}