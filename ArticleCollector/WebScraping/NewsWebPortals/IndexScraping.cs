using ElasticsearchClient.Models.News;
using HtmlAgilityPack;
using NLog;
using ScrapySharp.Extensions;
using ScrapySharp.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArticleCollector.WebScraping.NewsWebPortals
{
    /// <summary>
    /// Web scraping class used for retrieving articles from Index.hu.
    /// </summary>
    class IndexScraping : ScrapingBase
    {
        /// <summary>
        /// NLog log manager.
        /// </summary>
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// URL of Index.hu news web portal.
        /// </summary>
        private readonly Uri indexUrl;

        /// <summary>
        /// Initializes a new web scraping object for Index.hu with the given browser.
        /// </summary>
        public IndexScraping(ScrapingBrowser scrapingBrowser)
            : base(scrapingBrowser)
        {
            indexUrl = new Uri("https://index.hu/belfold/");
        }

        /// <summary>
        /// Retrieves articles from Index.hu by web scraping.
        /// </summary>
        /// <returns>list of articles from Index.hu</returns>
        public override List<Article> GetArticles()
        {
            Logger.Info("Getting articles from Index.hu via web scraping started.");

            scrapingBrowser.Encoding = Encoding.UTF8;

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

            // navigate to the collected articles' main pages to retrieve full text of article
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

                // get donation node ("nincs masik" widget)
                var donateNode = articleText.Descendants().Where(d => d.GetAttributeValue("class", "").Contains("nm_widget")).FirstOrDefault();

                var paragraphs = articleText.Descendants();

                if (paragraphs == null)
                {
                    Logger.Error("Article text paragraphs not found for {0}", article.Address);
                    continue;
                }

                foreach (var p in paragraphs)
                {
                    if (p.OriginalName == "p" || p.OriginalName.StartsWith("h") || p.OriginalName == "li")
                    {
                        // paragraphs, headings and lists

                        if (donateNode != null && donateNode.Descendants().Contains(p))
                        {
                            // this is not part of the article, but a campaign to donate money for the news portal
                            continue;
                        }

                        articleTextBuilder.AppendLine(p.InnerText.Trim());
                    }
                }

                // set article text from string builder
                article.Text = articleTextBuilder.ToString();
            }

            Logger.Info("{0} articles successfully retrieved from Index.hu.", articles.Count);
            return articles;
        }
    }
}