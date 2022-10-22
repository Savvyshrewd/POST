﻿using System.Net.Mail;
using POST.Core.Config;
using POST.Core.IMessage.Config;
using System.Net;

namespace POST.INFRASTRUCTURE.MessageService
{
    public class EmailSender
    {
        //private ILogger logger;
        private SmtpClient client;
        public IMessageConfig _config;
        private MailMessage mail;


        public EmailSender(IMessageConfig config)
        {
           _config = config;
            InitializeClient();
        }

        public void SendMessage(string address, string subject, string body, string sender)
        {
            mail = new MailMessage(sender, address);
            mail.Subject = subject;
            mail.Body = body;
            client.Send(mail);
        }

        public void InitializeClient()
        {
            client = new SmtpClient();
            EmailConfig _config = new EmailConfig();
            client.Host = _config.SmtpServerHost;
            client.Port = _config.SmtpPort;
            client.EnableSsl = true;
            var credentials = new NetworkCredential();
            credentials.UserName = _config.SenderEmail;
            credentials.Password = _config.SenderPassword;
            client.Credentials = credentials;
        }
  
    }
}
