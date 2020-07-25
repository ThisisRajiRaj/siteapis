using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.WebJobs;


namespace Rajirajcom.Api
{
    class FeedConfig
    {
        public int MaxItems { get; set; }
        public string RootUrl { get; set; }
        public string BlogUrl { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Language { get; set; }
        public string IndexFileLocation { get; set; }
        public string ContentFileRoot { get; set; }
        public bool EnableContent { get; set; }

        public FeedConfig(ExecutionContext context)
        {
            RootUrl = GetAppSettingOrDefault(context, "rootURL", null);
            BlogUrl = GetAppSettingOrDefault(context, "blogURL", null);
            Title = GetAppSettingOrDefault(context,
                "title",
                "Website feed");
            Description = GetAppSettingOrDefault(context,
                "description",
                "This is a generated blog feed");
            Language = GetAppSettingOrDefault(context,
                "language",
                "en");
            IndexFileLocation = GetAppSettingOrDefault(context,
                "indexfilelocation",
                null);
            ContentFileRoot = GetAppSettingOrDefault(context,
                "contentfileroot",
                RootUrl);
            MaxItems = Int32.Parse(GetAppSettingOrDefault(context, "maxitems","10"));
            EnableContent = 
                (GetAppSettingOrDefault(context, "enablecontent", "0") == "0" )
                ? false 
                : true;
        }

        private string GetAppSettingOrDefault(
            ExecutionContext context,
            string name,
            string defaultVal = "")
        {
            var config = new ConfigurationBuilder()
                           .SetBasePath(context.FunctionAppDirectory)
                           .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                           .AddEnvironmentVariables() // <- This is what actually gets you the application settings in Azure
                           .Build();
            if (config[name] == null && defaultVal == null)
            {
                throw new ApplicationException();
            }
            return config[name] ?? defaultVal;
        }

    }
}