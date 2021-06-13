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
        public async Task TestSingleEmptyFile_NoPath()
        {
            BlogManager bm = new BlogManager();
            List<BlogInfo> blogInfos = new List<BlogInfo>();
            try {
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
            finally {
                foreach (var blog in blogInfos) {                    
                    await bm.DeleteBlogIfExists(blog, connString);
                }
            }
        }

        [Test]
        public async Task TestWithInvalidPath()
        {
            BlogManager bm = new BlogManager();
            List<BlogInfo> blogInfos = new List<BlogInfo>();
            try {
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
            finally {
                foreach (var blog in blogInfos) {                    
                    await bm.DeleteBlogIfExists(blog, connString);
                }
            }
        }


        [Test]
        public async Task TestWith500Words()
        {            
            BlogManager bm = new BlogManager();
            List<BlogInfo> blogInfos = new List<BlogInfo>();
            try {
                blogInfos.Add(new BlogInfo
                {
                    Name = "fivehundredwords"
                });
                var response = bm.GetMinsToRead(blogInfos, connString, contentFileRoot);
                Task.WaitAll(response);
                Assert.AreEqual(1, response.Result.Count);
                Assert.AreEqual("1", response.Result[0].MinsToRead);
                Assert.AreEqual("fivehundredwords", response.Result[0].Name);
            }
            finally {
                foreach (var blog in blogInfos) {                    
                    await bm.DeleteBlogIfExists(blog, connString);
                }
            }
        }

        [Test]
        public async Task TestMultipleFileNames()
        {
            BlogManager bm = new BlogManager();
            List<BlogInfo> blogInfos = new List<BlogInfo>();
            try {           
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
                Assert.AreEqual("1", response.Result[0].MinsToRead);
                Assert.AreEqual("fivehundredwords", response.Result[0].Name);
            }
            finally {
                foreach (var blog in blogInfos) {                    
                    await bm.DeleteBlogIfExists(blog, connString);
                }
            }
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

        [Test]
        public async Task TestAddLike()
        {            
            BlogManager bm = new BlogManager();
            var blogInfo = new BlogInfo
            {
                Name = "addlike",
                Likes = 2
            };
            try {               
                var response = bm.Likes(blogInfo, connString, contentFileRoot);
                Task.WaitAll(response);
                Assert.AreEqual(2, response.Result);

                blogInfo.Likes = 1;                
                response = bm.Likes(blogInfo, connString, contentFileRoot);
                Task.WaitAll(response);
                Assert.AreEqual(3, response.Result);

                
                blogInfo.Likes = -1;                
                response = bm.Likes(blogInfo, connString, contentFileRoot);
                Task.WaitAll(response);
                Assert.AreEqual(2, response.Result);

                // Make like count go negative, the API should stop at 0
                blogInfo.Likes = -18;                
                response = bm.Likes(blogInfo, connString, contentFileRoot);
                Task.WaitAll(response);
                Assert.AreEqual(0, response.Result);
            }
            finally {
                await bm.DeleteBlogIfExists(blogInfo, connString);
            }
        }
        
        [Test]
        public async Task TestComments()
        {            
            BlogManager bm = new BlogManager();
            var blogInfo = new BlogInfo
            {
                Name = "comments",
                Comments = "Hello world 1"
            };
            try {               
                var response = bm.Comments(blogInfo, connString, contentFileRoot);
                Task.WaitAll(response);
                Assert.AreEqual("Hello world 1", response.Result);

                blogInfo.Comments = "Hello world 2";                
                response = bm.Comments(blogInfo, connString, contentFileRoot);
                Task.WaitAll(response);
                Assert.AreEqual("Hello world 2Hello world 1", response.Result);
            }
            finally {
                await bm.DeleteBlogIfExists(blogInfo, connString);
            }
        }

        
        [Test]
        public async Task TestGetBlogMetadata()
        {            
            BlogManager bm = new BlogManager();
            var blogInfo = new BlogInfo
            {
                Name = "getblogmetadata"
            };
            try {               
                var response = bm.GetBlogMetadata(blogInfo, connString, contentFileRoot);
                Task.WaitAll(response);
                var responseInfo= (BlogInfo)response.Result;
                Assert.Null(responseInfo.Comments);

                // pass in null comments                
                blogInfo.Comments =  null;                
                var comments = bm.Comments(blogInfo, connString, contentFileRoot);
                Task.WaitAll(comments);
                Assert.Null(comments.Result);

                blogInfo.Comments = "Hello world!";                
                comments = bm.Comments(blogInfo, connString, contentFileRoot);
                Task.WaitAll(comments);
                Assert.AreEqual("Hello world!", comments.Result);

                response = bm.GetBlogMetadata(blogInfo, connString, contentFileRoot);
                Task.WaitAll(response);
                responseInfo= (BlogInfo)response.Result;
                Assert.AreEqual("Hello world!", responseInfo.Comments);
            }
            finally {
                await bm.DeleteBlogIfExists(blogInfo, connString);
            }
        }
    }
}