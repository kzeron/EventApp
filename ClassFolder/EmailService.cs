using System;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MimeKit;

namespace EventApp.ClassFolder
{
    internal class EmailService
    {
        private readonly string _smtpServer;
        private readonly int _port;
        private readonly string _email;
        private readonly string _password;

        public EmailService(string smtpServer, int port, string email, string password)
        {
            _smtpServer = smtpServer;
            _port = port;
            _email = email;
            _password = password;
        }
        public async Task<bool> SendEmailAsync(string recipient, string subject, string body)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Отправитель", _email));
                message.To.Add(new MailboxAddress("Получатель", recipient));
                message.Subject = subject;

                message.Body = new TextPart("html") { Text = body };

                using(var client = new SmtpClient())
                {
                    await client.ConnectAsync(_smtpServer, _port, true);
                    await client.AuthenticateAsync(_email, _password);
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }
                return true;
            }
            catch(Exception ex)
            {
                MBClass.ErrorMB(ex);
                return false;
            }
        }
    }
}
