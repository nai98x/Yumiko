using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Discord_Bot
{
    public class MonoschinosDownloader
    {
        const string baseUrl = "https://monoschinos2.com/";

        public async Task<List<SearchResults>> Search(string searchText)
        {
            List<SearchResults> searchResults = new List<SearchResults>();
            string[] searchTextList = searchText.Split(' ');
            var url = baseUrl + "/search?q=";
            foreach (string word in searchTextList)
            {
                url += word + "+";
            }
            url = url.Remove(url.Length-1);
            var response = await GetHtml(url);

            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(response);
            var nodeResults = htmlDoc.DocumentNode.SelectNodes("//article[@class='col-6 col-sm-4 col-lg-2 mb-5']");

            if (nodeResults != null)
            {
                foreach (var node in nodeResults)
                {
                    searchResults.Add(
                        new SearchResults
                        {
                            href = node.Descendants("a").First().Attributes[1].Value.Replace("https://monoschinos2.com/anime", ""),
                            name = node.Descendants("h3").First().InnerText,
                            type = node.Descendants("span").ElementAt(1).InnerText
                        }
                    );
                }
            }
            return searchResults;
        }

        public async Task<AnimeLinks> GetLinks(string url, string name)
        {
            url = url.Replace("-sub-espanol", "");
            url = baseUrl + "ver" + url + "-episodio-";
            int episodeNumber = 1;
            bool nextEpisode = true;
            AnimeLinks result = new AnimeLinks() {
                name = name,
                hosts = new List<Host>()
            };

            while (nextEpisode)
            {
                var response = await GetHtml(url + episodeNumber);
                HtmlDocument htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(response);

                var downloadLinks = htmlDoc.DocumentNode.SelectNodes("//tbody//tr").ToList();

                downloadLinks.ForEach(tr =>
                {
                    var tdArray = tr.Descendants("td").ToArray();
                    string href = tdArray[2].Descendants("a").First().Attributes[1].Value;

                    if (!result.hosts.Exists(h => h.name == episodeNumber.ToString()))
                    {
                        result.hosts.Add(
                            new Host()
                            {
                                name = episodeNumber.ToString(),
                                links = new List<Link>()
                            }
                        );
                    }

                    result.hosts.Where(h => h.name == episodeNumber.ToString()).First().links.Add(
                        new Link()
                        {
                            number = episodeNumber,
                            href = href
                        }
                    );
                });

                nextEpisode = NextEpisodeExists(htmlDoc);
                episodeNumber++;
            }

            return result;
        }

        private async Task<string> GetHtml(string url)
        {
            HttpClient client = new HttpClient();
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            client.DefaultRequestHeaders.Accept.Clear();
            var response = await client.GetStringAsync(url);
            return response;
        }

        private bool NextEpisodeExists(HtmlDocument htmlDoc)
        {
            var next = htmlDoc.DocumentNode.SelectSingleNode("//a[@class='btnWeb']//i[@class='fas fa-arrow-circle-right']");
            if (next != null)
            {
                return true;
            }
            return false;
        }
    }
}
