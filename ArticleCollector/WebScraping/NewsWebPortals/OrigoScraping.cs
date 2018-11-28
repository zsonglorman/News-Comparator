using ElasticsearchClient.Models.News;
using ScrapySharp.Extensions;
using ScrapySharp.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace ArticleCollector.WebScraping.NewsWebPortals
{
    class OrigoScraping : ScrapingBase
    {
        /// <summary>
        /// URL of Origo.hu news web portal.
        /// </summary>
        private readonly Uri origoUrl;

        public OrigoScraping(ScrapingBrowser scrapingBrowser)
            : base(scrapingBrowser)
        {
            origoUrl = new Uri("http://www.origo.hu/itthon/index.html");
        }

        /// <summary>
        /// Retrieves articles from Origo.hu by web scraping.
        /// </summary>
        /// <returns>list of articles from Origo.hu</returns>
        public override List<Article> GetArticles()
        {
            var origoPage = scrapingBrowser.NavigateToPage(origoUrl);

            var articleNodes = origoPage.Html.CssSelect(".news-item");

            var articles = new List<Article>();
            foreach (var articleNode in articleNodes)
            {
                string address = articleNode.CssSelect("a.news-title").First().GetAttributeValue("href", "");

                if (address.Contains("/galeria/"))
                {
                    // this is jut a gallery, not an article, so we skip it
                    continue;
                }

                string title = articleNode.CssSelect("a.news-title").First().InnerText.Trim();
                string shortLead = articleNode.CssSelect("p.news-lead").First().InnerText.Trim();

                var article = new Article(address, title, shortLead);
                articles.Add(article);
            }

            // navigate to the collected articles' main pages to retrieve full text of article
            foreach (var article in articles)
            {
                var articlePage = scrapingBrowser.NavigateToPage(new Uri(article.Address));

                var articleTextBuilder = new StringBuilder();

                var articleTextNode = articlePage.Html.CssSelect("#article-text").First();
                var paragraphs = articleTextNode.Descendants("p");

                foreach (var p in paragraphs)
                {
                    // paragraphs, headers and lists
                    if (p.OriginalName == "p" || p.OriginalName.StartsWith("h") || p.OriginalName == "li")
                    {
                        // convert special HTML-encoded characters into decoded string
                        articleTextBuilder.AppendLine(WebUtility.HtmlDecode(p.InnerText.Trim()));
                    }
                }

                // set article text from string builder
                article.Text = articleTextBuilder.ToString();
            }

            return articles;
        }
    }
}