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

namespace Rajirajcom.Api
{
    public static partial class PostStats
    {
        [FunctionName("MinsToRead")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ExecutionContext context,
            ILogger log)
        {
            try {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                dynamic data = JsonConvert.DeserializeObject(requestBody);
                if (data == null)
                {
                    throw new ApplicationException(
                        "JSON object must be passed in"
                    );
                }
                var name = data?.name;
                var fileurl = data?.fileurl;

                if (name == null)
                {
                    throw new ApplicationException(
                        "Pass in a unique blogname.");
                }
                
                string connString = ConfigReader.GetAppSettingOrDefault(
                    context, 
                    "azurestorageconnstring",
                    null
                );
                BlogManager bm = new BlogManager();
                var mins = await bm.GetMinsToRead((string)name, 
                    (string)fileurl, 
                    connString,
                    context);

                return new ObjectResult(mins);

            }
            catch (Exception e)
            {
                return new ExceptionResult(e,true);
            }
        }
    }
}
