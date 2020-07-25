using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using System.Xml;
using System.ServiceModel.Syndication;

namespace Rajirajcom.Api
{
    public class FeedGeneratorTests
    {

        [Test]
        public void TestEmptyIndex()
        {
            FeedConfig c = new FeedConfig();
            c.RootUrl = "http://contoso.com";
            FeedGenerator sut = new FeedGenerator(c);
            sut.Indices = new List<FeedIndex>();
            Assert.AreEqual(sut.Indices.Count, 0);
            Task<IActionResult> items = sut.GetItems();
            Task.WaitAll(items);

            Assert.AreEqual(items.Result.GetType(), typeof(ContentResult));
            string content = ((ContentResult)items.Result).Content;
            var doc = new XmlDocument();
            doc.LoadXml(content);
            Assert.IsNull(doc.SelectSingleNode("//title").Value);
            Assert.AreEqual(doc.SelectSingleNode("//link").InnerText,
                "http://contoso.com/");
            ValidateRSS(content);
        }     

        [Test]
        public void TestItemsInIndex()
        {
            FeedConfig c = new FeedConfig();
            c.EnableContent = false;
            c.MaxItems = 10;
            c.Title = "Foobar";
            c.RootUrl = "http://contoso.com";
            c.BlogUrl = "http://contoso.com/blog";
            FeedGenerator sut = new FeedGenerator(c);
            var indices = new List<FeedIndex>();
            indices.Add(new FeedIndex{
                Name = "foo",
                Title = "this is foo",
                Date = "Sep 20, 2020"
            });
            indices.Add(new FeedIndex{
                Name = "bar",
                Title = "this is bar",
                Date = "Sep 20, 2018"
            });
            sut.Indices = indices;
            Task<IActionResult> items = sut.GetItems();
            Task.WaitAll(items);

            Assert.AreEqual(items.Result.GetType(), typeof(ContentResult));
            string content = ((ContentResult)items.Result).Content;
            var doc = new XmlDocument();
            doc.LoadXml(content);
            Assert.AreEqual(2, doc.SelectNodes("//item").Count);
            Assert.AreEqual("Foobar", doc.SelectSingleNode("//title").InnerText);
            Assert.AreEqual(doc.SelectSingleNode("//link").InnerText,
                "http://contoso.com/"); 
            ValidateRSS(content);
        }

        private static void ValidateRSS(string content)
        {
            Rss20FeedFormatter feedFormatter = new Rss20FeedFormatter();
            XmlReader rssReader = XmlReader.Create(new StringReader(content));
            if (feedFormatter.CanRead(rssReader))
            {
                feedFormatter.ReadFrom(rssReader);
                rssReader.Close();
            }
        }
    }
}