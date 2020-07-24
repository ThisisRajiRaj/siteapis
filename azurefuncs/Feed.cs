using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using System.ServiceModel.Syndication;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Xml;
using System.Text;
using System.Net.Http;
using Microsoft.Extensions.Configuration;

namespace Rajirajcom.Api
{
    public static class Feed
    {
        ///
        /// Index data structure to deserialize blog indices
        class Index
        {
            [JsonProperty(PropertyName = "name")]
            public string name { get; set; }
            [JsonProperty(PropertyName = "title")]
            public string title { get; set; }

            [JsonProperty(PropertyName = "date")]
            public string date { get; set; }
            [JsonProperty(PropertyName = "image")]
            public string image { get; set; }
        }
        private static HttpClient httpClient = new HttpClient();

        [FunctionName("Feed")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ExecutionContext context,
            ILogger log)
        {
            string indexLoc = GetAppSettingOrDefault(context,
                "indexfilelocation",
                "");

            var response = await httpClient.GetAsync(indexLoc);
            if (!response.IsSuccessStatusCode)
            {
                log.LogError($"{response.StatusCode} {response.ReasonPhrase}: ");
                return new NotFoundObjectResult("Cannot fetch index file");
            }
            SyndicationFeed feed = GetFeed(context);
            List<Index> indices = await GetIndices(response);
            List<SyndicationItem> items = new List<SyndicationItem>();
            for (int i = 0; i < indices.Count; i++)
            {
                Index index = indices[i];
                string contentRoot = GetAppSettingOrDefault(context,
                    "contentfileroot",
                    ""
                );
                items.Add(await GetItem(contentRoot, index));
            }
            feed.Items = items;

            Rss20FeedFormatter rssFormatter = new Rss20FeedFormatter(feed);
            var output = new StringBuilder();
            using (var writer = XmlWriter.Create(output, new XmlWriterSettings { Indent = true }))
            {
                rssFormatter.WriteTo(writer);
                writer.Flush();
                return new OkObjectResult(output.ToString());
            }
        }

        private static async Task<List<Index>> GetIndices(HttpResponseMessage response)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            List<Index> indices = JsonConvert.DeserializeObject<List<Index>>(responseContent);
            return indices;
        }

        private static async Task<SyndicationItem> GetItem(string contentFileRoot,
            Index index)
        {
            string link = string.Format("{0}/{1}.txt", contentFileRoot, index.name);
            HttpResponseMessage resp = await httpClient.GetAsync(link);
            string text = "";
            if (resp.IsSuccessStatusCode)
            {
                text = await resp.Content.ReadAsStringAsync();
                text = text.Substring(0, 200);
            }

            SyndicationItem item =
                new SyndicationItem(
                    index.title,
                    text,
                    new Uri(link),
                    link,
                    DateTime.Now
            );
            return item;
        }

        private async static void GetFileContent(
            Task<HttpResponseMessage> fileContentResponse,
            Index index
            )
        {
            string text = "";

            HttpResponseMessage resp = await fileContentResponse;
            if (resp.IsSuccessStatusCode)
            {
                text = await resp.Content.ReadAsStringAsync();
                text = text.Substring(0, 200);
            }
        }
        private static SyndicationFeed GetFeed(ExecutionContext context)
        {
            string url = GetAppSettingOrDefault(context, "rootURL");
            string title = GetAppSettingOrDefault(context,
                "title",
                "Website feed");
            string generator = GetAppSettingOrDefault(context,
                "generator",
                "Rajirajcom website feed generator");
            string description = GetAppSettingOrDefault(context,
                "description",
                "This is a generated blog feed");
            string language = GetAppSettingOrDefault(context,
                "language",
                "en");
            string copyright = GetAppSettingOrDefault(context,
                "copyright",
                "Copyright 2020");

            SyndicationFeed feed = new SyndicationFeed(
                title,
                description,
                new Uri(url),
                url,
                DateTime.Now
            );
            feed.Copyright = new TextSyndicationContent(copyright);
            feed.Generator = generator;
            feed.Language = language;
            return feed;
        }

        private static string GetAppSettingOrDefault(
            ExecutionContext context,
            string name,
            string defaultVal = "")
        {
            var config = new ConfigurationBuilder()
                           .SetBasePath(context.FunctionAppDirectory)
                           .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                           .AddEnvironmentVariables() // <- This is what actually gets you the application settings in Azure
                           .Build();
            return config[name] ?? defaultVal;
        }
    }
}
