using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using Microsoft.Azure.WebJobs;
using System.IO;
using System.Reflection;

namespace Rajirajcom.Api.Tests
{
    public class TestFactory
    {
        public static HttpRequest CreateHttpRequest(BlogInfo body)
        {
            var httpContext = new DefaultHttpContext();
  
            // Create the stream to house our content
            var ms = new MemoryStream();
            var sw = new StreamWriter(ms);
                
            var json = JsonConvert.SerializeObject(body);
            sw.Write(json);
            sw.Flush();
                
            ms.Position = 0;
            httpContext.Request.Body = ms;
            httpContext.Request.ContentLength = ms.Length;
            return httpContext.Request;
        }

        public static ExecutionContext GetExecutionContext(){
            ExecutionContext toRet = new ExecutionContext();
            toRet.FunctionAppDirectory =  Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            return toRet;
        }

        public static ILogger CreateLogger(LoggerTypes type = LoggerTypes.Null)
        {
            ILogger logger;

            if (type == LoggerTypes.List)
            {
                logger = new ListLogger();
            }
            else
            {
                logger = NullLoggerFactory.Instance.CreateLogger("Null Logger");
            }

            return logger;
        }
    }
}