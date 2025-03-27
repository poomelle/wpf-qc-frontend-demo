using ChemsonLabApp.Utilities;
using Microsoft.Office.Interop.Outlook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChemsonLabApp.Services.EmailServices
{
    public class OutlookEmailService : IEmailService.IEmailService
    {
        public async Task SendEmailAsync(string sender, string recipient, string subject, string body, List<string> attachments = null)
        {
            CursorUtility.DisplayCursor(true);
            try
            {
                await Task.Run(() =>
                {
                    Application outlookApp = new Application();
                    MailItem mailItem = (MailItem)outlookApp.CreateItem(OlItemType.olMailItem);

                    // sender
                    mailItem.SentOnBehalfOfName = sender ?? "";

                    // subject
                    mailItem.Subject = subject?? "";

                    // body
                    mailItem.Body = body ?? "";
                    mailItem.To = recipient ?? "";

                    if (attachments.Count > 0)
                    {
                        foreach (var attach in attachments)
                        {
                            mailItem.Attachments.Add(attach, OlAttachmentType.olByValue, Type.Missing, Type.Missing);
                        }
                    }

                    mailItem.Display(false);
                });
            }
            catch (System.Exception ex)
            {
                NotificationUtility.ShowError("Error while sending email");
                LoggerUtility.LogError(ex);
            }
            finally
            {
                CursorUtility.DisplayCursor(false);
            }
        }

        public void CreateReportEmailAndOpenOutlook(string subject, string imagePath, string sender)
        {
            // Create an Outlook Application instance
            var outlookApp = new Application();
            var mailItem = (MailItem)outlookApp.CreateItem(OlItemType.olMailItem);

            // Sender
            mailItem.SentOnBehalfOfName = sender;

            // Subject
            mailItem.Subject = subject;

            // Body Format
            mailItem.BodyFormat = OlBodyFormat.olFormatHTML;

            // Body Embed Image
            string contentID = Guid.NewGuid().ToString();
            var attachment = mailItem.Attachments.Add(imagePath, OlAttachmentType.olByValue, 0, "Test Report Image");
            attachment.PropertyAccessor.SetProperty("http://schemas.microsoft.com/mapi/proptag/0x3712001E", contentID);

            // Body Content
            mailItem.HTMLBody = $"<html><body><img src=\"cid:{contentID}\" alt=\"Test Report\"></body></html>";

            // Display the email in Outlook new message window
            mailItem.Display(false);
        }
    }
}
