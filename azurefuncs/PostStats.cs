using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Web.Http;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Rajirajcom.Api
{
    ///
    /// An Azure function to get stats about articles from their
    /// path
    ///
    public static partial class PostStats
    {
        ///
        /// An API to return an array containing Minutes-to-read article
        /// stats for passed in array of article names.
        /// 
        [FunctionName("MinsToRead")]
        public static async Task<IActionResult> MinsToRead(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ExecutionContext context,
            ILogger log)
        {
            try
            {
                JArray reqBodyData = await GetRequestArray(req);
                string connString = ConfigReader.GetAppSettingOrDefault(
                    context,
                    "azurestorageconnstring",
                    null
                ); string contentFileRoot = ConfigReader.GetAppSettingOrDefault(
                    context,
                    "contentfileroot",
                    null
                );

                BlogManager bm = new BlogManager();

                List<BlogInfo> reqData = reqBodyData.ToObject<List<BlogInfo>>();
                List<BlogInfo> toRet = await bm.GetMinsToRead(reqData,
                    connString,
                    contentFileRoot);
                return new OkObjectResult(toRet);
            }
            catch (Exception e)
            {
                return new ExceptionResult(e, true);
            }
        }

        ///
        /// An API to add and return all comments on a given blog name
        /// 
        [FunctionName("GetBlogMetadata")]
        public static async Task<IActionResult> GetBlogMetadata(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ExecutionContext context,
            ILogger log)
        {
             try
            {
                var reqBodyData = await GetRequestObject(req);
                string connString = ConfigReader.GetAppSettingOrDefault(
                    context,
                    "azurestorageconnstring",
                    null
                ); string contentFileRoot = ConfigReader.GetAppSettingOrDefault(
                    context,
                    "contentfileroot",
                    null
                );

                BlogManager bm = new BlogManager();

                BlogInfo reqData = reqBodyData.ToObject<BlogInfo>();
                BlogInfo toRet = await bm.GetBlogMetadata(reqData,
                    connString,
                    contentFileRoot);
                return new OkObjectResult(toRet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return new ExceptionResult(e, true);
            }
        }

        ///
        /// An API to add and return all comments on a given blog name
        /// 
        [FunctionName("UpdateLikes")]
        public static async Task<IActionResult> UpdateLikes(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ExecutionContext context,
            ILogger log)
        {
             try
            {
                var reqBodyData = await GetRequestObject(req);
                string connString = ConfigReader.GetAppSettingOrDefault(
                    context,
                    "azurestorageconnstring",
                    null
                ); string contentFileRoot = ConfigReader.GetAppSettingOrDefault(
                    context,
                    "contentfileroot",
                    null
                );

                BlogManager bm = new BlogManager();

                BlogInfo reqData = reqBodyData.ToObject<BlogInfo>();
                int toRet = await bm.AddLike(reqData,
                    connString,
                    contentFileRoot);
                return new OkObjectResult(toRet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return new ExceptionResult(e, true);
            }
        }

        ///
        /// An API to add and return all comments on a given blog name
        /// 
        [FunctionName("Comments")]
        public static async Task<IActionResult> Comments(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ExecutionContext context,
            ILogger log)
        {
            try
            {
                var reqBodyData = await GetRequestObject(req);
                string connString = ConfigReader.GetAppSettingOrDefault(
                    context,
                    "azurestorageconnstring",
                    null
                ); string contentFileRoot = ConfigReader.GetAppSettingOrDefault(
                    context,
                    "contentfileroot",
                    null
                );

                BlogManager bm = new BlogManager();

                BlogInfo reqData = reqBodyData.ToObject<BlogInfo>();
                string toRet = await bm.GetComments(reqData,
                    connString,
                    contentFileRoot);
                return new OkObjectResult(toRet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return new ExceptionResult(e, true);
            }
        }

        private static async Task<JArray> GetRequestArray(HttpRequest req)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            if (typeof(JArray).IsAssignableFrom(requestBody.GetType()))
            {
                throw new ApplicationException(
                    @"Pass in a json array with ""name"" and ""fileurl"" properties."
                );
            }

            JArray reqBodyData = (JArray)JsonConvert.DeserializeObject(requestBody);
            return reqBodyData;
        }

        private static async Task<JObject> GetRequestObject(HttpRequest req)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            if (typeof(JObject).IsAssignableFrom(requestBody.GetType()))
            {
                throw new ApplicationException(
                    @"Pass in a json object"
                );
            }

            JObject reqBodyData = (JObject)JsonConvert.DeserializeObject(requestBody);
            return reqBodyData;
        }

    }
}
