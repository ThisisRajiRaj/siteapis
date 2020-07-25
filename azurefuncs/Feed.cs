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
using System.IO;

namespace Rajirajcom.Api
{
    public static partial class Feed
    {
        private static HttpClient httpClient = new HttpClient();

        [FunctionName("Feed")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ExecutionContext context,
            ILogger log)
        {
            FeedConfig conf = new FeedConfig(context);
            FeedGenerator generator = new FeedGenerator(conf);
            return await generator.GetItems();
        }
    }
}
