using System.Linq;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;

namespace Rajirajcom.Api
{
    public class ConfigReader
    {
        public static string GetAppSettingOrDefault(
            ExecutionContext context,
            string name,
            string defaultVal = "")
        {
            var config = new ConfigurationBuilder()
                           .SetBasePath(context.FunctionAppDirectory)
                           .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                           .AddEnvironmentVariables() // <- This is what actually gets you the application settings in Azure
                           .Build();

            // If the key doesn't exist of the value is null, throw
            // error if no default val provided
            if (!config.GetChildren().Any(item => item.Key == name) ||
                config[name] == null)
            {
                if (defaultVal == null)
                {
                    throw new System.ApplicationException(
                        string.Format("Error reading {0} from application settings", name)
                    );
                }
            }
            return config[name] ?? defaultVal;
        }

        public static string GetFileContentURL(
            string pagesRootURL,
            string filename)
        {
            return string.Format("{0}/{1}.html",
               pagesRootURL,
               filename);
        }
    }

}