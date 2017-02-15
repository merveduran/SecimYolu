using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Web;

namespace secimyolu.Models
{
    public class SecimMail
    {
        /// <summary>
        /// Kuyruktaki mailleri göndermeyi sağlıyor.
        /// </summary>
        /// <param name="email">gönderilecek kişinin adresi</param>
        /// <param name="subject">konu</param>
        /// <param name="body">içerik</param>
        /// <returns>bool</returns>
        public static void MailGonder(int MailTo, string MailSubject, string MailContent, int MailId)
        {
            Mail sendMail = Current.Context.Mail.FirstOrDefault(d => d.id == MailId);
            try
            {
                if (sendMail != null)
                {
                    sendMail.Status = Constants.MAIL_STATUS_SEND;
                    MailMessage message = new MailMessage();
                    message.From = new MailAddress(ConfigurationSettings.AppSettings["SenderMailAdress"].ToString(), ConfigurationSettings.AppSettings["SenderDisplayName"].ToString());
                    message.To.Add(new MailAddress(Current.Context.User.FirstOrDefault(d => d.Id == MailTo).Email));
                    message.Subject = MailSubject;
                    message.Body = MailContent;
                    message.IsBodyHtml = true;
                
                    SmtpClient client = new SmtpClient(ConfigurationSettings.AppSettings["Host"].ToString(), Convert.ToInt32(ConfigurationSettings.AppSettings["Port"]));
                    client.EnableSsl = true;
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;
                    NetworkCredential NTLMAuthentication = new NetworkCredential(ConfigurationSettings.AppSettings["Username"].ToString(), ConfigurationSettings.AppSettings["Password"].ToString());
                    client.UseDefaultCredentials = false;
                    client.Credentials = NTLMAuthentication;
                    client.Send(message);
                }
            }
            catch (Exception ex)
            {
                if (sendMail != null)
                {
                    sendMail.Status = Constants.MAIL_STATUS_FAIL;
                    MailError m = new MailError
                    {
                        ErrorMessage = ex.Message,
                        Time = DateTime.Now,
                        MailId = MailId
                    };
                    Current.Context.MailError.Add(m);
                    Current.Context.SaveChanges();
                }
            }

        }
    }
}