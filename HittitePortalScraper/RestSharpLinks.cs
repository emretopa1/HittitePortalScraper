using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using RestSharp;
using HtmlAgilityPack;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using System.Net;
using System.Web;
using System.Text.RegularExpressions;

namespace Test
{
    public class RestSharpLinks
    {
        private readonly RestClient restClient;


        public RestSharpLinks()
        {
            // Configure RestSharp client
            var options = new RestClientOptions
            {
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36",
                FollowRedirects = true,
                MaxRedirects = 5,
                ThrowOnAnyError = false,
                Timeout = TimeSpan.FromSeconds(30)
            };

            restClient = new RestClient(options);
        }

        /// <summary>
        /// Get all <a href> links from a webpage
        /// </summary>
        public async Task<List<string>> GetAllLinksAsync(string url, string xpath)
        {
            try
            {
                Console.WriteLine($"Fetching: {url}");

                // Download the webpage using RestSharp
                var request = new RestRequest(url);
                var response =  restClient.Execute(request);

                if (!response.IsSuccessful)
                {
                    Console.WriteLine($"Failed: {response.StatusCode} - {response.ErrorMessage}");
                    return new List<string>();
                }

                // Parse HTML using HtmlAgilityPack
                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(response.Content);
                var links = new List<string>();
                // Extract all <a> tags with href attribute
                if (url== $@"https://www.hethport.uni-wuerzburg.de/CTH/")
                {
                    links = htmlDoc.DocumentNode
                        .SelectNodes(xpath)?
                        .Select(n => n.GetAttributeValue("href", ""))
                        .Where(href =>
                        {
                            var uri = new Uri("https://dummy.com" + href); 
                            var query = System.Web.HttpUtility.ParseQueryString(uri.Query);

                            return int.TryParse(query["c"], out int cValue) && cValue > 0;
                        })
                        .ToList() ?? new List<string>();
                }
                else
                {
                    links = htmlDoc.DocumentNode
                                    .SelectNodes(xpath)
                                    ?.Select(node => node.GetAttributeValue("href", string.Empty))
                                    .Where(href => !string.IsNullOrWhiteSpace(href) && href.Contains("bildpraep"))
                                    .ToList() ?? new List<string>();

                }

                Console.WriteLine($"✓ Found {links.Count} links");
                return links;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return new List<string>();
            }
        }
        public async Task<List<string>> GetAllLinksAsyncd(string url, string xpath)
        {
            try
            {
                Console.WriteLine($"Fetching: {url}");

                // Download the webpage using RestSharp
                var request = new RestRequest(url);
                var response = restClient.Execute(request);

                if (!response.IsSuccessful)
                {
                    Console.WriteLine($"Failed: {response.StatusCode} - {response.ErrorMessage}");
                    return new List<string>();
                }

                // Parse HTML using HtmlAgilityPack
                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(response.Content);

                // Extract all <img> tags with src attribute
                var links = htmlDoc.DocumentNode
                    .SelectNodes(xpath)
                    ?.Select(node => node.GetAttributeValue("href", string.Empty))
                    .Where(href => !string.IsNullOrWhiteSpace(href))
                    .ToList() ?? new List<string>();

                Console.WriteLine($"✓ Found {links.Count} links");
                return links;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return new List<string>();
            }
        }
        public async Task<List<string>> GetAllLinksAsync(List<string> links)
        {
            try
            {
                var listLinks = new List<string>();
                foreach (var link in links)
                {

                    var uri = new Uri("https://dummy.com/" + link);

                    // Parse query string
                    var queryParams = HttpUtility.ParseQueryString(uri.Query);

                    string fundnr = queryParams["fundnr"];

                    var options = new RestClientOptions("https://www.hethport.adwmainz.de")
                    {
                        ThrowOnAnyError = false,
                        CookieContainer = new CookieContainer()
                    };
                    var client = new RestClient(options);
                    var request1 = new RestRequest("fotarch/bildausw2.php", Method.Get);
                    request1.AddQueryParameter("n", fundnr);
                    var response1 = await client.ExecuteAsync(request1);
                    var responseUrl = response1.ResponseUri.ToString();
                    listLinks.Add(responseUrl);
                }
                Console.WriteLine($"✓ Found {listLinks.Count} links");
                return listLinks;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return new List<string>();
            }
        }

    }
}
