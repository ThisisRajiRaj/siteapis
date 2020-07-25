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
            ILogger log)
        {
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                dynamic data = JsonConvert.DeserializeObject(requestBody);
                if (data == null)
                {
                    throw new ApplicationException(
                        "JSON object must be passed in: "
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
                var result = Execute(fullMsg, devflag.ToString() ?? "true");
                return new OkObjectResult("Success");
            }
            catch (Exception e)
            {
                string msg = $"Failed to send email with subject " +
                              "'{email.Subject}' to Administrator";
                log.LogError(e.ToString());
                return new ExceptionResult(e, true);
            }
        }

        static async Task<string> Execute(string msg, string devflag)
        {
            var apiKey = "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";
            var client = new SendGridClient(apiKey);
            var subject = "RajiRaj.com Contact Form ";
            var to = new EmailAddress("rajigopal@gmail.com", "Raji Rajagopalan");
            var from = new EmailAddress("rajigopal@gmail.com", "Raji Rajagopalan");
            var email = MailHelper.CreateSingleEmail(from, to, subject, msg, "");
            if (devflag == "true") return "success";
            var res = await client.SendEmailAsync(email);
            return res.Body.ToString();
            
        }
    }
}
