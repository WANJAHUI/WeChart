using Microsoft.Extensions.Options;
using SqsMessageHandle.EnumAndConsts;
using SqsMessageHandle.Services.S3;
using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace SqsMessageHandle.Services.Email
{
   
    public    class SmtpEmailService
    {
        private static SmtpClient _client;
        private static MailOptions _options;
        private S3SimpleOperator _s3SimpleOperator;
        public SmtpEmailService(IOptions<MailOptions> options, S3SimpleOperator s3SimpleOperator)
        {
            _options = options.Value;
            _s3SimpleOperator = s3SimpleOperator;
            _client = new SmtpClient
            {
                Host = _options.Host,
                Port =  _options.Port > 0 ? Convert.ToInt32(_options.Port) : 25,
                UseDefaultCredentials = false,
                Credentials = new System.Net.NetworkCredential(_options.UserName, _options.Password),
                DeliveryMethod = SmtpDeliveryMethod.Network,
                EnableSsl = _options.EnableSsl,
                Timeout = 30000
            };

        }
        public  async Task<(bool,string)> SendAttachmentEmailAsync(string email, string subject, ContentType type,string body,
        List<string> attachments, string cc = null,string bcc=null)
        {
            MailMessage mail = CreateMail(email, subject, type, body, cc,bcc);

            if (attachments != null)
            {
                foreach (string attachment in attachments)
                {
                    var stream = await _s3SimpleOperator.GetFileStreamAsync(attachment);
                    var name = attachment.Substring(attachment.IndexOf('_') + 1);
                    mail.Attachments.Add(new Attachment(attachment));
                }
            }
            await _client.SendMailAsync(mail);
            return (true, "");
        }
        private  MailMessage CreateMail(string to, string subject, ContentType type , string body, string cc,string bcc )
        {
            MailMessage mail = new MailMessage
            {
                From = new MailAddress(_options.Email, _options.UserName),
                Subject = subject,
                Body = type==ContentType.Text? body: get_uft8(body),
                IsBodyHtml = type == ContentType.Html,
                SubjectEncoding = Encoding.UTF8,
                BodyEncoding = Encoding.UTF8
            };
            mail.To.Add(to);

            if (!string.IsNullOrEmpty(cc))
            {
                mail.CC.Add(cc);//多个用,分割
            }
            if (!string.IsNullOrEmpty(bcc))
            {
                mail.Bcc.Add(bcc);//多个用,分割
            }
            return mail;
        }
        public string get_uft8(string unicodeString)
        {

            UTF8Encoding utf8 = new UTF8Encoding();
            Byte[] encodedBytes = utf8.GetBytes(unicodeString);
            String decodedString = utf8.GetString(encodedBytes);
            return decodedString;

        }

    }
}
