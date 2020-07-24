using System;
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
using System.IO;
using System.Linq;

namespace Rajirajcom.Api
{
    public class Utf8StringWriter : StringWriter
    {
        public override Encoding Encoding
        {
            get { return Encoding.UTF8; }
        }
    }
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

            indices.Sort((a, b) => (
                DateTime.Parse(b.date).CompareTo(DateTime.Parse(a.date))
            ));
            List<Index> filtered = FilterToMax(context, indices);
            await GetFeedItem(context, feed, filtered);
            return GetFeedResult(feed);
        }

        private static List<Index> FilterToMax(ExecutionContext context,
        List<Index> indices)
        {
            int max = Int32.Parse(GetAppSettingOrDefault(context, "maxitems","10"));
            List<Index> filtered = new List<Index>();
            for (int i = 0; i < max; i++)
            {
                if (i >= indices.Count) break;
                filtered.Add(indices[i]);
            }

            return filtered;
        }

        private static async Task GetFeedItem(ExecutionContext context, 
            SyndicationFeed feed, 
            List<Index> indices)
        {
            List<SyndicationItem> items = new List<SyndicationItem>();
            for (int i = 0; i < indices.Count; i++)
            {
                Index index = indices[i];
                items.Add(await GetItem(context, index));
            }
            feed.Items = items;
            feed.Language = GetAppSettingOrDefault(context,
                "language",
                "en");
        }

        private static IActionResult GetFeedResult(SyndicationFeed feed)
        {
            Rss20FeedFormatter rssFormatter = new Rss20FeedFormatter(feed);

            using (TextWriter utf8 = new Utf8StringWriter())
            {
                using (var writer = XmlWriter.Create(utf8,
                    new XmlWriterSettings
                    {
                        Indent = true,
                        Encoding = Encoding.GetEncoding("utf-8")
                    }))
                {
                    rssFormatter.WriteTo(writer);
                    writer.Flush();
                    return new ContentResult
                    {
                        Content = utf8.ToString(),
                        ContentType = "application/rss+xml"
                    };
                }
            }
        }

        private static async Task<List<Index>> GetIndices(HttpResponseMessage response)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            List<Index> indices = JsonConvert.DeserializeObject<List<Index>>(responseContent);
            return indices;
        }

        private static async Task<SyndicationItem> GetItem(ExecutionContext context,            
            Index index)
        {
            string text = "";

            if (GetAppSettingOrDefault(context, "enablecontent", "0") == "1")
            {
                string filepath = GetAppSettingOrDefault(context, 
                    "contentfileroot");                    
                HttpResponseMessage resp = await httpClient.GetAsync(
                    string.Format("{0}/{1}.txt", filepath, index.name));
                if (resp.IsSuccessStatusCode)
                {
                    text = await resp.Content.ReadAsStringAsync();
                }
            }
            
            string blogpath = GetAppSettingOrDefault(context, 
                    "blogURL"); 
            string link = string.Format("{0}/{1}", blogpath, index.name);
            var pubDate = DateTime.Parse(index.date).ToUniversalTime();
            SyndicationItem item =
                new SyndicationItem(
                    index.title,
                    text,
                    new Uri(link),
                    link,
                    pubDate                               
            );
            item.Content =  SyndicationContent.CreateHtmlContent(text.Substring(0,2000));
            item.PublishDate = pubDate;
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
