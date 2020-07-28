using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using Microsoft.Extensions.Configuration;
using System;
using System.Configuration;
using System.Runtime;

namespace Rajirajcom.Api
{
    public class BlogManagerTests
    {
        private string connString;
        private string contentFileRoot;
        [SetUp]
        public void Setup()
        {
            var config = new ConfigurationBuilder()
                 .AddJsonFile("clientsettings.json")
                 .Build();
            connString = config["azurestorageconnstring"];
            contentFileRoot = config["contentfileroot"];
        }

        [Test]
        public void TestSingleEmptyFile_NoPath()
        {
            BlogManager bm = new BlogManager();
            List<BlogInfo> blogInfos = new List<BlogInfo>();
            blogInfos.Add(new BlogInfo
            {
                Name = "empty"
            });
            var response = bm.GetMinsToRead(blogInfos, connString, contentFileRoot);
            Task.WaitAll(response);
            Assert.AreEqual(1, response.Result.Count);
            Assert.AreEqual("0", response.Result[0].MinsToRead);
            Assert.AreEqual("empty", response.Result[0].Name);
        }

        [Test]
        public void TestWithInvalidPath()
        {
            BlogManager bm = new BlogManager();
            List<BlogInfo> blogInfos = new List<BlogInfo>();
            blogInfos.Add(new BlogInfo
            {
                Name = "invalid",
                Path = "thiswontwork"
            });
            var except = Assert.Throws<AggregateException>(() =>
            {
                var response = bm.GetMinsToRead(blogInfos, connString, contentFileRoot);
                Task.WaitAll(response);
            });
            StringAssert.Contains("An invalid request URI was provided.", except.Message);
        }


        [Test]
        public void TestWith500Words()
        {
            BlogManager bm = new BlogManager();
            List<BlogInfo> blogInfos = new List<BlogInfo>();
            blogInfos.Add(new BlogInfo
            {
                Name = "fivehundredwords"
            });
            var response = bm.GetMinsToRead(blogInfos, connString, contentFileRoot);
            Task.WaitAll(response);
            Assert.AreEqual(1, response.Result.Count);
            Assert.AreEqual("2", response.Result[0].MinsToRead);
            Assert.AreEqual("fivehundredwords", response.Result[0].Name);
        }

        [Test]
        public void TestMultipleFileNames()
        {
            BlogManager bm = new BlogManager();
            List<BlogInfo> blogInfos = new List<BlogInfo>();
            blogInfos.Add(new BlogInfo
            {
                Name = "fivehundredwords"
            });
            blogInfos.Add(new BlogInfo
            {
                Name = "empty"
            });
            var response = bm.GetMinsToRead(blogInfos, connString, contentFileRoot);
            Task.WaitAll(response);
            Assert.AreEqual(2, response.Result.Count);
            Assert.AreEqual("2", response.Result[0].MinsToRead);
            Assert.AreEqual("fivehundredwords", response.Result[0].Name);
        }
        [Test]
        public void TestNullName()
        {
            BlogManager bm = new BlogManager();
            List<BlogInfo> blogInfos = new List<BlogInfo>();
            blogInfos.Add(new BlogInfo
            {
            });
            var except = Assert.Throws<AggregateException>(() =>
            {
                var response = bm.GetMinsToRead(blogInfos, connString, contentFileRoot);
                Task.WaitAll(response);
            });
            StringAssert.Contains("Pass in a unique blogname in the list of names", except.Message);        
        }
    }
}