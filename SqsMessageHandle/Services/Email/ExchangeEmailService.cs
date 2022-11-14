using Microsoft.Exchange.WebServices.Data;
using Microsoft.Extensions.Options;
using SqsMessageHandle.EnumAndConsts;
using SqsMessageHandle.Services.S3;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SqsMessageHandle.Services.Email
{

    public class ExchangeEmailservice
    {
        private MailOptions _options;
        private S3SimpleOperator   _s3SimpleOperator ;
        public ExchangeEmailservice(IOptions<MailOptions> options, S3SimpleOperator s3SimpleOperator)
        {
            _options = options.Value;
            _s3SimpleOperator = s3SimpleOperator;

        }
        public string get_uft8(string unicodeString)
        {

            UTF8Encoding utf8 = new UTF8Encoding();
            Byte[] encodedBytes = utf8.GetBytes(unicodeString);
            String decodedString = utf8.GetString(encodedBytes);
            return decodedString;

        }
        public async Task<(bool, string)> SendEmailByExchange(string Subject, ContentType type, string Body, string to, string cc, string bcc,List<string> AttachmentList)
        {
            var tolist = to.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            if (tolist.Length == 0)
                return (false,"发送对象为空");

           
                var service = new ExchangeService();
                service.Timeout = 200000;
                service.Credentials = new NetworkCredential(_options.UserName, password: _options.Password, _options.Email);
                service.TraceEnabled = true;
                service.Url = new Uri(_options.Host);

                var message = new EmailMessage(service);
                message.From = _options.From;
                message.Subject = Subject;
                if (type == ContentType.Html)
                    message.Body = new MessageBody(BodyType.HTML, this.get_uft8(Body));
                else
                    message.Body = new MessageBody(Body);

                //收件人
                message.ToRecipients.AddRange(tolist);
                //抄送人
                if (!string.IsNullOrEmpty(cc))
                {
                    var cclist = cc.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);

                    message.CcRecipients.AddRange(cclist);

                }
            //抄送人
            if (!string.IsNullOrEmpty(bcc))
            {
                var bcclist = bcc.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);

                message.BccRecipients.AddRange(bcclist);

            }

            //附件
            if (AttachmentList!=null&&AttachmentList.Count != 0)
                {
                    foreach (string attachment in AttachmentList)
                    {
                        var stream = await _s3SimpleOperator.GetFileStreamAsync(attachment);
                        var name = attachment.Substring(attachment.IndexOf('_') + 1);
                        message.Attachments.AddFileAttachment(name, stream);
                    }
                }

              await  message.SendAndSaveCopy();
                //LogHelper.WriteLog($"JobTicket({ticket})邮件发送【成功】({threadticket})");

                return (true, "");
         
        }



    }
}
