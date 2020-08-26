
using LORLib.Core;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Web;

namespace FV_API_Harness_Tester
{
    public class bizEmail
    {


        // Logging
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private delegate void SendEmailDelegate(System.Net.Mail.MailMessage m);


        public static Boolean SendEmailTo(String subject, String message, String recipientEmail, string basePath, string senderEmail = null, List<string> emailAttachmentFiles = null, string Bcc = "")
        {
            Boolean retVal = false;

            try
            {
                System.Net.Mail.MailMessage emailMsg = new System.Net.Mail.MailMessage();

                string sendEmailMode = ConfigurationManager.AppSettings["SendEmailMode"].ToString().ToLower();


                if (sendEmailMode != "none")
                {

                    if (sendEmailMode == "live")
                    {
                        String[] emailAddresses = recipientEmail.Split(';');
                        for (int i = emailAddresses.GetLowerBound(0); i <= emailAddresses.GetUpperBound(0); i++)
                        {
                            if (emailAddresses[i].Trim() != "")
                            {
                                emailMsg.To.Add(emailAddresses[i]);
                            }
                        }

                        if (!string.IsNullOrEmpty(Bcc))
                        {
                            emailMsg.Bcc.Add(Bcc);
                        }

                    }
                    else if (sendEmailMode == "test")
                    {
                        String[] emailAddresses = ConfigurationManager.AppSettings["TestEmail"].ToString().Split(';');
                        for (int i = emailAddresses.GetLowerBound(0); i <= emailAddresses.GetUpperBound(0); i++)
                        {
                            if (emailAddresses[i].Trim() != "")
                            {
                                emailMsg.To.Add(emailAddresses[i]);
                            }
                        }
                        //if (!string.IsNullOrEmpty(Bcc))
                        //{
                        //    emailMsg.Bcc.Add(Bcc);
                        //}
                    }

                    if (emailMsg.To.Count < 1)
                    {
                        emailMsg = null;
                        return false;
                    }

                    List<string> linkedImages = new List<string>();
                    int conID = 0;
                    while (message.Contains("<img src=\"/"))
                    {
                        int sPos = message.IndexOf("<img src=\"/");
                        int ePos = message.IndexOf("\"", sPos + 10);
                        var sImg = message.Substring(sPos + 5, ePos - sPos - 4);
                        string fileUrl = sImg.Substring(5, sImg.Length - 6);
                        conID++;
                        string contentID = "img" + conID.ToString();

                        linkedImages.Add(fileUrl);

                        message = message.Replace(sImg, "src=\"cid:" + contentID + "\"");
                    }

                    string FromEmail = senderEmail == null ? ConfigurationManager.AppSettings["ApplicationEmail"].ToString() : "Customer Care System On behalf of <" + senderEmail + ">";
                    // emailMsg.From = new System.Net.Mail.MailAddress(Utils.SystemAppSettings("ApplicationEmail"));
                    emailMsg.From = new System.Net.Mail.MailAddress(FromEmail);
                    emailMsg.Subject = subject;
                    emailMsg.Priority = System.Net.Mail.MailPriority.Normal;
                    System.Net.Mail.AlternateView htmlView = System.Net.Mail.AlternateView.CreateAlternateViewFromString(message, null, MediaTypeNames.Text.Html);

                    conID = 0;
                    foreach (string fileImg in linkedImages)
                    {
                        conID++;
                        string fileName = basePath + fileImg.Replace("/", @"\");
                        var imageLink = new LinkedResource(fileName, "image/jpg")
                        {
                            ContentId = "img" + conID.ToString(),
                            TransferEncoding = TransferEncoding.Base64
                        };
                        htmlView.LinkedResources.Add(imageLink);
                    }
                    emailMsg.AlternateViews.Add(htmlView);

                    if (emailAttachmentFiles != null)
                    {
                        if (emailAttachmentFiles.Count > 0)
                        {
                            foreach (string emailAttachment in emailAttachmentFiles)
                            {
                                Attachment a = new Attachment(emailAttachment);
                                emailMsg.Attachments.Add(a);
                            }
                        }
                    }

                    sendEmail(emailMsg, false);

                    retVal = true;
                }
            }
            catch (Exception)
            {
                retVal = false;
            }
            return retVal;
        }

        /// <summary>
        /// Send email
        /// </summary>
        /// <param name="m"></param>
        /// <param name="Async"></param>
        private static void sendEmail(System.Net.Mail.MailMessage m, Boolean Async)
        {
            System.Net.Mail.SmtpClient smtpClient = null;
            smtpClient = new System.Net.Mail.SmtpClient(ConfigurationManager.AppSettings["Application_SMTPServer"]);
            if (Async)
            {
                SendEmailDelegate sd = new SendEmailDelegate(smtpClient.Send);
                AsyncCallback cb = new AsyncCallback(SendEmailResponse);
                sd.BeginInvoke(m, cb, sd);
            }
            else
            {
                log.Debug("Sending the email non-async mode");
                smtpClient.Send(m);
            }
        }

        private static void SendEmailResponse(IAsyncResult ar)
        {
            SendEmailDelegate sd = (SendEmailDelegate)(ar.AsyncState);

            sd.EndInvoke(ar);
        }


    }
}