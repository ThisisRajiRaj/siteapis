using System.Reflection;
using Microsoft.Extensions.Logging;
using Xunit;
using Newtonsoft.Json;
using System.Collections.Generic;
using Xunit.Abstractions;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using System;
using Microsoft.Azure.WebJobs;

namespace Rajirajcom.Api.Tests
{
    public class PostsStatsTest
    {
         private readonly ITestOutputHelper output;

        private readonly ILogger logger = TestFactory.CreateLogger();

        class LocalSettings
        {
            public bool IsEncrypted { get; set; }
            public Dictionary<string, string> Values { get; set; }
        }

        private static void SetupEnvironment()
        {
            string basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var settings = JsonConvert.DeserializeObject<LocalSettings>(
                File.ReadAllText(basePath + "\\local.settings.json"));

            foreach (var setting in settings.Values)
            {
                Environment.SetEnvironmentVariable(setting.Key, setting.Value);
            }
        }
        public PostsStatsTest(ITestOutputHelper output)
        {
            this.output = output;
            SetupEnvironment();
        }

        [Fact]
        public async void GetBlogMetadata_TestBlob ()
        {   
            ExecutionContext context = TestFactory.GetExecutionContext();
            BlogInfo info = new BlogInfo() {
                Name="GetBlogMetadata_TestBlob" + new Random(101)
            };
            string connString = ConfigReader.GetAppSettingOrDefault(
                    context,
                    "azurestorageconnstring",
                    null
            ); 
            try 
            {
                // Setup blob
                var request = TestFactory.CreateHttpRequest(info);
                var response = (OkObjectResult)await PostStats.GetBlogMetadata(request, context, logger);
                Assert.Equal(200, response.StatusCode);

                BlogInfo responseObj = (BlogInfo)response.Value;
                Assert.Null(responseObj.Comments);
                Assert.Equal(0, responseObj.Likes);

                // New request to update likes
                info.Likes = 2;
                var likeReq = TestFactory.CreateHttpRequest(info);
                var likeResp = (OkObjectResult)await PostStats.UpdateLikes(likeReq, context, logger);
                
                Assert.Equal(200, likeResp.StatusCode);
                int likes = (int)likeResp.Value;
                Assert.Equal(2, likes);

                // New request to update likes to decrement
                info.Likes = -2;
                var unlikeReq = TestFactory.CreateHttpRequest(info);
                var unlikeResp = (OkObjectResult)await PostStats.UpdateLikes(unlikeReq, context, logger);
                
                Assert.Equal(200, unlikeResp.StatusCode);
                likes = (int)unlikeResp.Value;
                Assert.Equal(0, responseObj.Likes);
                
                // New request to add comments. Note this call should not affect likes
                info.Likes = 1;
                info.Comments = "This is a test";
                var commReq = TestFactory.CreateHttpRequest(info);
                var commResp = (OkObjectResult)await PostStats.Comments(commReq, context, logger);
                
                Assert.Equal(200, commResp.StatusCode);
                string comments = (string)commResp.Value;
                Assert.NotNull(comments);
                Assert.Equal(info.Comments, comments);

                // One more request to add comment
                info.Comments = "This is test 2";
                string expectedComment = info.Comments + "This is a test";
                commReq = TestFactory.CreateHttpRequest(info);
                commResp = (OkObjectResult)await PostStats.Comments(commReq, context, logger);
                
                Assert.Equal(200, commResp.StatusCode);
                comments = (string)commResp.Value;
                Assert.NotNull(comments);
                Assert.Equal(expectedComment, comments);


                // Now get all of the metadata together                
                request = TestFactory.CreateHttpRequest(info);
                response = (OkObjectResult)await PostStats.GetBlogMetadata(request, context, logger);
                Assert.Equal(200, response.StatusCode);

                responseObj = (BlogInfo)response.Value;
                Assert.Equal(expectedComment, responseObj.Comments);
                Assert.Equal(0, responseObj.Likes);

            }
            finally 
            {
                // Delete blog
                await new BlogManager().DeleteBlogIfExists(info, connString);
            }
        }
   }
}