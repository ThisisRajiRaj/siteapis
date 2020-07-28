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
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ExecutionContext context,
            ILogger log)
        {
            try
            {
                JArray reqBodyData = await GetRequest(req);
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

        private static async Task<JArray> GetRequest(HttpRequest req)
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

    }
}
