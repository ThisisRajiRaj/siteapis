using System;
using Microsoft.Azure.WebJobs;

namespace Rajirajcom.Api
{
    class SendMaildConfig
    {
        public string FromName {get; set;}
        public string ToName {get; set;}
        public string EmailFrom { get; set; }
        public string EmailTo { get; set; }
        public string EmailSubject { get; set; }
        public string SendGridApiKEy { get; set; }
        public SendMaildConfig(ExecutionContext context)
        {
            FromName = ConfigReader.GetAppSettingOrDefault(context, "fromname", null);
            ToName = ConfigReader.GetAppSettingOrDefault(context, "toname", FromName);
            EmailFrom = ConfigReader.GetAppSettingOrDefault(context, "emailfrom", null);
            EmailTo = ConfigReader.GetAppSettingOrDefault(context, "emailto", EmailFrom);
            EmailSubject = ConfigReader.GetAppSettingOrDefault(context,
                "title",
                "New email from " + EmailFrom);
            SendGridApiKEy = ConfigReader.GetAppSettingOrDefault (context,
                "sendgridappkey",
                null);
        }

    }
}