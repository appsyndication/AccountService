using System;
using System.Net;
using System.Net.Mail;
using AppSyndication.AccountService.Web.Models;
using BrockAllen.MembershipReboot;

namespace AppSyndication.AccountService.Web.Services
{
    public class ConfigurableMessageDelivery : IMessageDelivery
    {
        public ConfigurableMessageDelivery(MailConfig mailConfig)
        {
            this.MailConfig = mailConfig;
        }

        private MailConfig MailConfig { get; }

        public void Send(Message msg)
        {
            var from = String.IsNullOrEmpty(msg.From) ? this.MailConfig.DefaultFrom : msg.From;

            using (var smtp = new SmtpClient(this.MailConfig.Host, this.MailConfig.Port))
            {
                smtp.Credentials = new NetworkCredential(this.MailConfig.UserName, this.MailConfig.Password);
                smtp.Timeout = this.MailConfig.Timeout;

                var message = new MailMessage(from, msg.To, msg.Subject, msg.Body)
                {
                    //    IsBodyHtml = this.SendAsHtml
                };

                smtp.Send(message);
            }
        }
    }
}
