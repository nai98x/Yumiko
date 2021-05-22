namespace Discord_Bot
{
    using HtmlAgilityPack;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;

    public class AnimeFLVDownloader
    {
        const string baseUrl = "https://www3.animeflv.net";

        public async Task<List<SearchResults>> Search(string searchText)
        {
            List<SearchResults> searchResults = new List<SearchResults>();
            string[] searchTextList = searchText.Split(' ');
            var url = baseUrl + "/browse?q=";
            foreach (string word in searchTextList)
            {
                url += word + "+";
            }
            url = url.Remove(url.Length-1);
            var response = await GetHtml(url);

            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(response);
            var nodeResults = htmlDoc.DocumentNode.SelectNodes("//ul[@class='ListAnimes AX Rows A03 C02 D02']//li");

            if (nodeResults != null)
            {
                foreach (var node in nodeResults)
                {
                    searchResults.Add(
                        new SearchResults
                        {
                            href = node.Descendants("a").First().Attributes[0].Value.Replace("/anime", ""),
                            name = node.Descendants("h3").First().InnerText,
                            type = node.Descendants("span").First().InnerText
                        }
                    );
                }
            }
            return searchResults;
        }

        public async Task<AnimeLinks> GetLinks(string url, string name)
        {
            url = baseUrl + "/ver" + url + "-";
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
                    string hostName = tdArray[0].InnerText;
                    string language = tdArray[2].InnerText;
                    string href = tdArray[3].Descendants("a").First().Attributes[2].Value;

                    if (!result.hosts.Exists(h => h.name == hostName && h.language == language))
                    {
                        result.hosts.Add(
                            new Host()
                            {
                                name = hostName,
                                language = language,
                                links = new List<Link>()
                            }
                        );
                    }

                    result.hosts.Where(h => h.name == hostName && h.language == language).First().links.Add(
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
            var next = htmlDoc.DocumentNode.SelectSingleNode("//a[@class='CapNvNx fa-chevron-right']");
            if (next != null)
            {
                return true;
            }
            return false;
        }
    }
}
