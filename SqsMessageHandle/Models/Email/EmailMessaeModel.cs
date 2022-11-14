using SqsMessageHandle.EnumAndConsts;
using System;
using System.Collections.Generic;
using System.Text;

namespace SqsMessageHandle.Models
{
   public class EmailMessaeModel
    {
        public MailSendType sendtype { get; set; } = MailSendType.Exchange;
        /// <summary>
        /// 收件人
        /// </summary>
        public string touser { get; set; }
        public string subject { get; set; }
        public string content { get; set; }
        public ContentType contenttype { get; set; } = ContentType.Html;

        /// <summary>
        /// 抄送人 ","号隔开
        /// </summary>
        public string cc { get; set; }
        public string bcc { get; set; }
        /// <summary>
        /// 附件列表  放在S3上面
        /// </summary>
        public List<string> attachments { get; set; }
    }
  
}
