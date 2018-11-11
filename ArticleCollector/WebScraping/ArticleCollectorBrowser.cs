using ArticleCollector.Models;
using HtmlAgilityPack;
using ScrapySharp.Extensions;
using ScrapySharp.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace ArticleCollector.WebScraping
{
    /// <summary>
    /// Web scraping browser used for collecting news articles.
    /// </summary>
    class ArticleCollectorBrowser
    {
        /// <summary>
        /// The browser by ScrapySharp used internally for web scraping.
        /// </summary>
        private ScrapingBrowser scrapingBrowser;

        /// <summary>
        /// URL of Index.hu news web portal.
        /// </summary>
        private readonly Uri indexUrl;

        /// <summary>
        /// URL of Hirado.hu news web portal.
        /// </summary>
        private readonly Uri hiradoUrl;

        /// <summary>
        /// URL of Origo.hu news web portal.
        /// </summary>
        private readonly Uri origoUrl;

        /// <summary>
        /// Initializes a new web scraping browser for collecting articles.
        /// </summary>
        public ArticleCollectorBrowser()
        {
            scrapingBrowser = new ScrapingBrowser()
            {
                Encoding = Encoding.UTF8
            };

            // initialize URLs for news portals
            indexUrl = new Uri("https://index.hu/belfold/");
            hiradoUrl = new Uri("https://www.hirado.hu/belfold/");
            origoUrl = new Uri("http://www.origo.hu/itthon/index.html");
        }

        /// <summary>
        /// Retrieves articles from different news portals by web scraping.
        /// </summary>
        /// <returns>list of articles retrieved</returns>
        public List<Article> GetArticlesFromNewsPortals()
        {
            // get articles from different web portals
            var allArticles = GetArticlesFromIndex();
            var articlesFromHirado = GetArticlesFromHirado();
            var articlesFromOrgio = GetArticlesFromOrigo();

            // merge list of articles into one
            allArticles.AddRange(articlesFromHirado);
            allArticles.AddRange(articlesFromOrgio);

            return allArticles;
        }

        /// <summary>
        /// Retrieves articles from Index.hu by web scraping.
        /// </summary>
        /// <returns>list of articles from news portal</returns>
        private List<Article> GetArticlesFromIndex()
        {
            var indexPage = scrapingBrowser.NavigateToPage(indexUrl);

            var articleNodes = indexPage.Html.CssSelect(".rovatajanlo");

            var articles = new List<Article>();
            foreach (var articleNode in articleNodes)
            {
                HtmlNode titleNode = null;
                if (articleNode.GetAttributeValue("class", "").Contains("vezeto"))
                {
                    // this is the first news article, it has a different title (headline)
                    titleNode = articleNode.CssSelect("h1 a").First();
                }
                else
                {
                    titleNode = articleNode.CssSelect("h1 .cim").First();
                }

                // get address from href attribute of anchor HTML tag
                string address = titleNode.GetAttributeValue("href", "");

                if (address.Contains("/mindekozben/"))
                {
                    // mindeközben posts are not traditional news articles, so we skip it
                    continue;
                }

                string title = titleNode.InnerText.Trim();
                string shortLead = articleNode.CssSelect(".ajanlo").First().InnerText.Trim();

                var article = new Article(address, title, shortLead);
                articles.Add(article);
            }

            // navigate to the collected articles' main pages
            foreach (var article in articles)
            {
                var articlePage = scrapingBrowser.NavigateToPage(new Uri(article.Address));

                if (articlePage.Html.InnerHtml.Contains("<div class=\"allapot elo\">Élő</div>")
                    || articlePage.Html.InnerHtml.Contains("<div class=\"allapot vege\">Vége</div>"))
                {
                    // this is a Live page, which is not a traditional news article, so we skip it
                    continue;
                }
                
                var articleTextBuilder = new StringBuilder();

                var lead = articlePage.Html.CssSelect(".lead");
                if (lead.Count() > 0)
                {
                    // there is a lead in the article main page (not always), add it to the text content of the article
                    articleTextBuilder.AppendLine(lead.First().InnerText.Trim());
                }

                HtmlNode articleText = articlePage.Html.CssSelect(".cikk-torzs").First();

                foreach (var p in articleText.Descendants())
                {
                    if (p.OriginalName == "p" || p.OriginalName.StartsWith("h") || p.OriginalName == "li")
                    {
                        // paragraphs, headings and lists
                        articleTextBuilder.AppendLine(p.InnerText.Trim());
                    }
                }

                // set article text from string builder
                article.Text = articleTextBuilder.ToString();
            }

            return articles;
        }

        /// <summary>
        /// Retrieves articles from Hirado.hu by web scraping.
        /// </summary>
        /// <returns>list of articles from news portal</returns>
        private List<Article> GetArticlesFromHirado()
        {
            var hiradoPage = scrapingBrowser.NavigateToPage(hiradoUrl);

            var articleNodes = hiradoPage.Html.CssSelect(".listerTxt");

            var articles = new List<Article>();
            foreach (var articleNode in articleNodes)
            {
                // get address from href attribute of anchor HTML tag
                string address = articleNode.CssSelect("a").First().GetAttributeValue("href", "");
                address = address.Replace("//www.hirado.hu", "https://www.hirado.hu");

                string title = articleNode.CssSelect("h4").First().InnerText.Trim();
                string shortLead = articleNode.CssSelect("div.Txt").First().InnerText.Trim();
                // convert special HTML-encoded characters into decoded string (e.g. &hellip; = ...)
                shortLead = WebUtility.HtmlDecode(shortLead);

                var article = new Article(address, title, shortLead);
                articles.Add(article);
            }

            // navigate to the collected articles' main pages
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

        /// <summary>
        /// Retrieves articles from Origo.hu by web scraping.
        /// </summary>
        /// <returns>list of articles from news portal</returns>
        private List<Article> GetArticlesFromOrigo()
        {
            var origoItthon = scrapingBrowser.NavigateToPage(origoUrl);

            var articleNodes = origoItthon.Html.CssSelect(".news-item");

            var articles = new List<Article>();
            foreach (var articleNode in articleNodes)
            {
                string address = articleNode.CssSelect("a.news-title").First().GetAttributeValue("href", "");

                if (address.Contains("/galeria/"))
                {
                    // no article, just gallery, skip it
                    continue;
                }

                string title = articleNode.CssSelect("a.news-title").First().InnerText.Trim();
                string shortLead = articleNode.CssSelect("p.news-lead").First().InnerText.Trim();

                var article = new Article(address, title, shortLead);
                articles.Add(article);
            }

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