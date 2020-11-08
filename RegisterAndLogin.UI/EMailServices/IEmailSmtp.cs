using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegisterAndLogin.UI.EMailServices
{
    public interface IEmailSmtp
    {
        Task SendEmailAsync(string email, string subject, string messageBody);
    }
}
