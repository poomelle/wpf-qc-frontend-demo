using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChemsonLabApp.Services.EmailServices.IEmailService
{
    public interface IEmailService
    {
        Task SendEmailAsync(string sender, string recipient, string subject, string body, List<string> attachment = null);
        void CreateReportEmailAndOpenOutlook(string subject, string imagePath, string sender);
    }
}
