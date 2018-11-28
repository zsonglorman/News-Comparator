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
    /// <summary>
    /// Web scraping class used for retrieving articles from Hirado.hu.
    /// </summary>
    class HiradoScraping : ScrapingBase
    {
        /// <summary>
        /// URL of Hirado.hu news web portal.
        /// </summary>
        private readonly Uri hiradoUrl;

        /// <summary>
        /// Initializes a new web scraping object for Hirado.hu with the given browser.
        /// </summary>
        public HiradoScraping(ScrapingBrowser scrapingBrowser)
            : base(scrapingBrowser)
        {
            hiradoUrl = new Uri("https://www.hirado.hu/belfold/");
        }


        /// <summary>
        /// Retrieves articles from Hirado.hu by web scraping.
        /// </summary>
        /// <returns>list of articles from Hirado.hu</returns>
        public override List<Article> GetArticles()
        {
            scrapingBrowser.Encoding = Encoding.UTF8;

            var hiradoPage = scrapingBrowser.NavigateToPage(hiradoUrl);

            var articleNodes = hiradoPage.Html.CssSelect(".listerTxt");

            var articles = new List<Article>();
            foreach (var articleNode in articleNodes)
            {
                // get address from href attribute of anchor HTML tag
                string address = articleNode.CssSelect("a").First().GetAttributeValue("href", "");
                address = address.Replace("//www.hirado.hu", "https://www.hirado.hu") + "/";

                string title = articleNode.CssSelect("h4").First().InnerText.Trim();
                string shortLead = articleNode.CssSelect("div.Txt").First().InnerText.Trim();
                // convert special HTML-encoded characters into decoded string (e.g. &hellip; = ...)
                shortLead = WebUtility.HtmlDecode(shortLead);

                var article = new Article(address, title, shortLead);
                articles.Add(article);
            }

            // navigate to the collected articles' main pages to retrieve full text of article
            foreach (var article in articles)
            {
                var articlePage = scrapingBrowser.NavigateToPage(new Uri(article.Address));

                var articleTextBuilder = new StringBuilder();

                var articleTextNode = articlePage.Html.CssSelect(".articleContent").First();
                var paragraphs = articleTextNode.Descendants("p");

                foreach (var p in paragraphs)
                {
                    if (p.OriginalName == "p" || p.OriginalName == "li")
                    {
                        // paragraphs and lists
                        articleTextBuilder.AppendLine(p.InnerText.Trim());
                    }
                }

                // set article text from string builder
                article.Text = articleTextBuilder.ToString();
            }

            return articles;
        }
    }
}