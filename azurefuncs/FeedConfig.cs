using System;
using Microsoft.Azure.WebJobs;

namespace Rajirajcom.Api
{
    public class FeedConfig
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

        public FeedConfig()
        {
            // For testing. Create an empty object
        }
        public FeedConfig(ExecutionContext context)
        {
            RootUrl = ConfigReader.GetAppSettingOrDefault(context, "rootURL", null);
            BlogUrl = ConfigReader.GetAppSettingOrDefault(context, "blogURL", null);
            Title = ConfigReader.GetAppSettingOrDefault(context,
                "title",
                "Website feed");
            Description = ConfigReader.GetAppSettingOrDefault(context,
                "description",
                "This is a generated blog feed");
            Language = ConfigReader.GetAppSettingOrDefault(context,
                "language",
                "en");
            IndexFileLocation = ConfigReader.GetAppSettingOrDefault(context,
                "indexfilelocation",
                null);
            ContentFileRoot = ConfigReader.GetAppSettingOrDefault(context,
                "contentfileroot",
                RootUrl);
            MaxItems = Int32.Parse(
                ConfigReader.GetAppSettingOrDefault(context, "maxitems","10")
            );
            EnableContent = 
                (ConfigReader.GetAppSettingOrDefault(context, "enablecontent", "0") == "0" )
                ? false 
                : true;
        }

    }
}