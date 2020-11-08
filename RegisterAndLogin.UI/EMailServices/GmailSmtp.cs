using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace RegisterAndLogin.UI.EMailServices
{
    public class GmailSmtp : IEmailSmtp
    {
        private string _host;
        private int _port;
        private bool _enableSSL;
        private string _username;
        private string _password;

        public GmailSmtp(string host, int port, bool enableSSL, string username, string password)
        {
            this._host = host;
            this._port = port;
            this._enableSSL = enableSSL;
            this._username = username;
            this._password = password;
        }
        public Task SendEmailAsync(string email, string subject, string messageBody)
        {
            var client = new SmtpClient(this._host, this._port)
            {
                Credentials = new NetworkCredential(this._username, this._password),
                EnableSsl = this._enableSSL
            };

            return client.SendMailAsync(
                new MailMessage(this._username, email, subject, messageBody)
                {
                    IsBodyHtml = true
                }
        );
       }
    }
}
