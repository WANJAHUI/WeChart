using System;
using System.Collections.Generic;
using System.Text;

namespace SqsMessageHandle.Models.WorkWechat
{
   public class WorkWechatTextMessageModel
    {
        public string text { get; set; }
        public string toparty { get; set; }
        public string touser { get; set; }
        public string totag { get; set; }
        public int safe { get; set; }
    }
    public class WorkWechatMarkdownModel
    {
        public string text { get; set; }
        public string toparty { get; set; }
        public string touser { get; set; }
        public string totag { get; set; }
        public int safe { get; set; }
    }
}
