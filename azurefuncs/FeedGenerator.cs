using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.ServiceModel.Syndication;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Xml;
using System.Text;
using System.Net.Http;
using System.IO;


namespace Rajirajcom.Api
{
    public class FeedGenerator
    {   private static HttpClient httpClient = new HttpClient();
        private static readonly int MAX_CONTENT_LENGTH = 2000;
        private FeedConfig config;
        public FeedGenerator(FeedConfig config)
        {
            this.config = config;
        }

        public List<FeedIndex> Indices {
            get;
            set;
        }
        
        // TODO: To Test, create a feed generator object
        // Pass in config with enablecontent == 0, 
        // Set Indices to the test array
        // and call GetItems
        // Need to figure out how to test enablecontent
        public async Task<IActionResult> GetItems()
        {
            if (Indices == null) {
                Indices = await GetIndices();
            }
            Indices.Sort((a, b) => (
                DateTime.Parse(b.Date).CompareTo(DateTime.Parse(a.Date))
            ));
            List<FeedIndex> filtered = FilterToMax(Indices);

            SyndicationFeed feed = GetFeed();
            await GetFeedItem(feed, filtered);
            return GetFeedResult(feed);
        }

        private async Task<List<FeedIndex>> GetIndices()
        {
            var response = await httpClient.GetAsync(this.config.IndexFileLocation);
            if (!response.IsSuccessStatusCode)
            {
                throw new ApplicationException("Cannot find index");
            }
            return await GetIndices(response);
        }
        private SyndicationFeed GetFeed()
        {
            SyndicationFeed feed = new SyndicationFeed(
                config.Title,
                config.Description,
                new Uri(config.RootUrl),
                config.RootUrl,
                DateTime.Now
            );            
            feed.Language = config.Language;
            return feed;
        }
        
        private static async Task<List<FeedIndex>> GetIndices(HttpResponseMessage response)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            List<FeedIndex> indices = JsonConvert.DeserializeObject<List<FeedIndex>>(responseContent);
            return indices;
        }

        private List<FeedIndex> FilterToMax(List<FeedIndex> indices)
        {
            List<FeedIndex> filtered = new List<FeedIndex>();
            for (int i = 0; i < config.MaxItems; i++)
            {
                if (i >= indices.Count) break;
                filtered.Add(indices[i]);
            }
            return filtered;
        }
        
        private async Task GetFeedItem(             
            SyndicationFeed feed, 
            List<FeedIndex> indices)
        {
            List<SyndicationItem> items = new List<SyndicationItem>();
            for (int i = 0; i < indices.Count; i++)
            {
                FeedIndex index = indices[i];
                items.Add(await GetItem(index));
            }
            feed.Items = items;
            feed.Language = config.Language;
        }


        private async Task<SyndicationItem> GetItem(FeedIndex index)
        {
            string text = "";
            if (config.EnableContent)
            {
                string filepath = config.ContentFileRoot;                    
                HttpResponseMessage resp = await httpClient.GetAsync(
                    string.Format("{0}/{1}.html", filepath, index.Name));
                if (resp.IsSuccessStatusCode)
                {
                    text = await resp.Content.ReadAsStringAsync();
                }
            }
            
            string blogpath = config.BlogUrl;
            string link = string.Format("{0}/{1}", blogpath, index.Name);
            var pubDate = DateTime.Parse(index.Date).ToUniversalTime();
            SyndicationItem item =
                new SyndicationItem(
                    index.Title,
                    text,
                    new Uri(link),
                    link,
                    pubDate                               
            );

            if (config.EnableContent) {
                string substr = text.Length > MAX_CONTENT_LENGTH 
                    ? text.Substring(0,MAX_CONTENT_LENGTH)
                    : text;
                item.Content =  SyndicationContent.CreateHtmlContent(substr);
            }
            item.PublishDate = pubDate;
            return item;
        }
        private IActionResult GetFeedResult(SyndicationFeed feed)
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

    }
}