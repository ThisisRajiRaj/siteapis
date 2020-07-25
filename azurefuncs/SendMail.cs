using System;
using System.IO;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Rajirajcom.Api
{
    public static class SendMail
    {
        [FunctionName("SendMail")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ExecutionContext context,
            ILogger log)
        {
            try
            {                
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                dynamic data = JsonConvert.DeserializeObject(requestBody);
                if (data == null)
                {
                    throw new ApplicationException(
                        "JSON object must be passed in"
                    );
                }
                var fromName = data?.from;
                var fromEmail = data?.fromemail;
                var msg = data?.message;
                var devflag = data?.devflag;

                var fullMsg = string.Format("From: {0}\nFrom Email:{1}\nMessage: {2}",
                 fromName,
                 fromEmail,
                 msg);
                var result = Execute(context, fullMsg, devflag ?? "true");
                return new OkObjectResult("Success");
            }
            catch (Exception e)
            {                
                return new ExceptionResult(e, true);
            }
        }

        static async Task<string> Execute(
            ExecutionContext context,
            string msg, 
            dynamic devflag)
        {
            SendMailConfig config = new SendMailConfig(context);
            var apiKey = config.SendGridApiKey;
            var client = new SendGridClient(apiKey);
            return await GetEmailContent(msg, devflag, config, client);
        }

        private static async Task<string> GetEmailContent(
            string msg, 
            dynamic devflag, 
            SendMailConfig config, 
            SendGridClient client)
        {
            var shldNotSend = devflag.ToString();
            var subject = config.EmailSubject;
            var to = new EmailAddress(config.EmailTo, config.FromName);
            var from = new EmailAddress(config.EmailFrom, config.ToName);
            var email = MailHelper.CreateSingleEmail(from, to, subject, msg, "");
            if (shldNotSend == "true") return "success";
            var res = await client.SendEmailAsync(email);
            return res.Body.ToString();
        }
    }
}